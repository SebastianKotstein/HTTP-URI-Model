// MIT License
//
// Copyright (c) 2020 Sebastian Kotstein
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skotstein.net.http.urimodel.openapi
{
    /// <summary>
    /// The class <see cref="UriModelFactory"/> implements methods for creating a <see cref="UriModel"/> by parsing a given OpenAPI documentation.
    /// This class implements the Singleton pattern. Use <see cref="Instance"/> to get an instance of this class.
    /// </summary>
    public class UriModelFactory
    {
        private static UriModelFactory _instance;

        /// <summary>
        /// Gets an instance of this class
        /// </summary>
        public static UriModelFactory Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new UriModelFactory();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        protected UriModelFactory()
        {

        }

        /// <summary>
        /// Converts the passed <see cref="OpenApiDocument"/> into a <see cref="UriModel"/> and returns this created <see cref="UriModel"/>.
        /// </summary>
        /// <param name="openApiDocument"></param>
        /// <returns>the created <see cref="UriModel"/></returns>
        public UriModel Create(OpenApiDocument openApiDocument)
        {
            UriModel uriModel = new UriModel();
            uriModel.Root = GeneratePathSegment("","/"); //the value of the API root should always be empty

            foreach(KeyValuePair<string, OpenApiPathItem> pathItem in openApiDocument.Paths)
            {
                
                foreach (KeyValuePair<OperationType, OpenApiOperation> operation in pathItem.Value.Operations)
                {
                    AddOperationToUriModel(pathItem.Key, pathItem.Value, operation.Key, operation.Value, uriModel);
                }

            }
            return uriModel;
        }

        /// <summary>
        /// Loads an OpenAPI documentation, which is located behind the passed path, and converts it into a <see cref="UriModel"/>. The method returns this created <see cref="UriModel"/>.
        /// </summary>
        /// <param name="openApiFilePath">the path pointing to the OpenAPI documentation</param>
        /// <returns>the created <see cref="UriModel"/></returns>
        public UriModel Create(string openApiFilePath)
        {
            OpenApiDiagnostic diagnostic = new OpenApiDiagnostic();
            OpenApiDocument openApiDocument = new OpenApiStringReader().Read(File.ReadAllText(openApiFilePath), out diagnostic);
            return Create(openApiDocument);
        }

        /// <summary>
        /// The method converts the passed <see cref="OpenApiOperation"/> into an <see cref="Operation"/> and adds this <see cref="Operation"/> to the passed <see cref="UriModel"/>.
        /// Moreover, the method requires the <see cref="OpenApiPathItem"/>
        /// 
        /// The methods adds an <see cref="Operation"/> to the passed <see cref="UriModel"/>
        /// </summary>
        /// <param name="uriPath"></param>
        /// <param name="pathItem"></param>
        /// <param name="httpMethod"></param>
        /// <param name="operation"></param>
        /// <param name="uriModel"></param>
        protected void AddOperationToUriModel(string uriPath, OpenApiPathItem pathItem, OperationType httpMethod, OpenApiOperation operation, UriModel uriModel)
        {
            //special case, if uri path is '/' (API Root)
            if (uriPath.CompareTo("/") == 0)
            {
                AddOperationToPathSegment(uriModel.Root, httpMethod, operation);
                return;
            }

            //split path into segments
            string[] segments = uriPath.Split('/');

            //start at root
            PathSegment currentPathSegment = uriModel.Root;

            //iterate over all segments (skip whitespaces and empty segments, especially the first segment which is always "", since URI paths starts with a leading slash '/')
            for(int i = 0; i < segments.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(segments[i]))
                {
                    //try to load the path segment that is attached to the current path segment (if existing) and matches the given segment name
                    PathSegment child = (PathSegment)currentPathSegment.GetChildWithValue(segments[i]);

                    //if such a path segment exist 
                    if(child != null)
                    {
                        //set context to this child
                        currentPathSegment = child;
                    }
                    else
                    {
                        //if such a path segment does not exist yet, create a new path segment:

                        //the value is the segment name
                        string value = segments[i];
                        //create URI path up to this path segment
                        string path = UriModel.BuildFullPath(segments, i + 1);

                        PathSegment newPathSegment = GeneratePathSegment(value, path);

                        //search for path parameters:
                        string indexedSegment = "";
                        foreach(PathParameter pathParameter in IdentifyAndFindPathParametersInOpenApiDocumentation(value, out indexedSegment, operation, pathItem))
                        {
                            pathParameter.PathSegment = newPathSegment;
                            newPathSegment.PathParameters.Add(pathParameter);
                        }
                        //add the segment string containing the positions of the path parameters (e.g. 'a[$1]b[$2]:c[$3]' for the segment 'a{id}b{x}:c{id})
                        newPathSegment.IndexedValue = indexedSegment;

                        //add new segment as child to the current segment
                        currentPathSegment.AddChild(newPathSegment);
                        //set context to this new segment
                        currentPathSegment = newPathSegment;

                    }
                }
            }
            //finally, add operation:
            AddOperationToPathSegment(currentPathSegment, httpMethod, operation);

        }

        /// <summary>
        /// Adds the passed <see cref="OpenApiOperation"/> to the passed <see cref="PathSegment"/>. The method checks whether there exists already an <see cref="Operation"/> having the same <see cref="OperationType"/> (if so, the method throws an exception).
        /// </summary>
        /// <param name="pathSegment"></param>
        /// <param name="httpMethod"></param>
        /// <param name="operation"></param>
        protected void AddOperationToPathSegment(PathSegment pathSegment, OperationType httpMethod, OpenApiOperation operation)
        {
            Operation uriModelOperation = GenerateOperation(operation, httpMethod, pathSegment);

            //check whether the operation already exists for the identified last path segment
            if (!pathSegment.SupportsOperation(uriModelOperation.Method))
            {
                pathSegment.Operations.Add(uriModelOperation);
            }
            else
            {
                throw new Exception("Cannot add Operation '" + httpMethod + "' to " + pathSegment.UriPath + " since the Operation has already been added before. Check the OpenAPI document for duplicate operations.");
            }
        }

        /// <summary>
        /// The method scans the passed 'segment' string (first argument) for parameters matching the path parameter pattern. For each identified path parameter,
        /// the method tries to find the path parameter description in the OpenAPI documentation, which is provided as the third and fouth argument.
        /// Note that the second argument 'indexedSegment' is an out type, which documents the position of the identified and returned path parameters within the original segment
        /// (e.g. 'a[$1]b[$2]:c[$3]' for the segment 'a{id}b{x}:c{id}).
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="indexedSegment"></param>
        /// <param name="operation"></param>
        /// <param name="pathItem"></param>
        /// <returns></returns>
        protected IList<PathParameter> IdentifyAndFindPathParametersInOpenApiDocumentation(string segment, out string indexedSegment, OpenApiOperation operation, OpenApiPathItem pathItem)
        {
            IList<PathParameter> pathParameters = new List<PathParameter>();

            //extract path parameters from segment name and interate over the list of extracted path parameters
            indexedSegment = "";
            foreach (PathParameter pathParameter in PathParameter.ExtractPathParameter(segment, out indexedSegment))
            {
                //search for path parameter description in the OpenAPI documentation:
                OpenApiParameter openApiParameter = null;

                //start to search in the OpenAPI operation description:
                foreach(OpenApiParameter parameter in operation.Parameters)
                {
                    if(parameter.Name.CompareTo(pathParameter.ParameterName)==0 && parameter.In == ParameterLocation.Path)
                    {
                        openApiParameter = parameter;
                    }
                }

                //if not found yet, continue search in the OpenAPI path description:
                if(openApiParameter == null)
                {
                    foreach(OpenApiParameter parameter in pathItem.Parameters)
                    {
                        if(parameter.Name.CompareTo(pathParameter.ParameterName)==0 && parameter.In == ParameterLocation.Path)
                        {
                            openApiParameter = parameter;
                        }
                    }
                }

                //check whether documented path parameter has been found:
                if(openApiParameter == null)
                {
                    throw new Exception("The variable Path Segment '" + segment + "' should be added, but one of its path parameter is not documented in the OpenAPI documentation");
                }
                else
                {
                    PathParameter oApiPathParameter = GeneratePathParameter(openApiParameter);
                    oApiPathParameter.ParameterName = pathParameter.ParameterName;
                    oApiPathParameter.Index = pathParameter.Index;
                    pathParameters.Add(oApiPathParameter);
                }
            }
            return pathParameters;
        }

        protected virtual PathSegment GeneratePathSegment(string value, string uriPath)
        {
            return new PathSegment(value, uriPath);
        }

        protected virtual PathParameter GeneratePathParameter(OpenApiParameter parameter)
        {
            return new OApiPathParameter(parameter);
        }

        protected Operation GenerateOperation(OpenApiOperation operation, OperationType type, PathSegment pathSegment)
        {
            HttpMethod method;
            switch (type)
            {
                case OperationType.Delete:
                    method = HttpMethod.DELETE;
                    break;
                case OperationType.Get:
                    method = HttpMethod.GET;
                    break;
                case OperationType.Head:
                    method = HttpMethod.HEAD;
                    break;
                case OperationType.Options:
                    method = HttpMethod.OPTIONS;
                    break;
                case OperationType.Patch:
                    method = HttpMethod.PATCH;
                    break;
                case OperationType.Post:
                    method = HttpMethod.POST;
                    break;
                case OperationType.Put:
                    method = HttpMethod.PUT;
                    break;
                case OperationType.Trace:
                    method = HttpMethod.TRACE;
                    break;
                default:
                    method = HttpMethod.GET;
                    break;
            }
            return new OApiOperation(pathSegment, method, operation);
        }
    }
}
