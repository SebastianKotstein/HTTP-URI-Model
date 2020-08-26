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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skotstein.net.http.urimodel
{
    /// <summary>
    /// An instance of this class represents the URI model and contains a tree like structure consisting of <see cref="PathSegment"/>s as tree nodes and <see cref="Operation"/>s as attributes.
    /// </summary>
    public class UriModel
    {
        private PathSegment _root;

        /// <summary>
        /// Gets or sets the API Root of the URI model.
        /// </summary>
        public PathSegment Root
        {
            get
            {
                return _root;
            }
            set
            {
                _root = value;
            }
        }

        /// <summary>
        /// The method calculates the tree path from the API Root to the passed <see cref="PathSegment"/> and returns a list of all
        /// <see cref="PathSegment"/> on the tree path. The first element in the returned list is the API Root and the last element is the passed <see cref="PathSegment"/>.
        /// </summary>
        /// <param name="lastPathSegment"></param>
        /// <param name="addRootAsFirstElement"></param>
        /// <returns></returns>
        public IList<PathSegment> GetPathSegmentsByLastPathSegment(PathSegment lastPathSegment)
        {
            Stack<Node<string>> stack = Root.Path(lastPathSegment);
            IList<PathSegment> pathSegments = new List<PathSegment>();
            while (stack.Count > 0)
            {
                PathSegment pathSegment = (PathSegment)stack.Pop();
                pathSegments.Add(pathSegment);
            }
            return pathSegments;
        }

        /// <summary>
        /// The method maps the passed stable URI path to the underlying <see cref="UriModel"/>. The method returns a list of <see cref="KeyValuePair{TKey, TValue}"/>s
        /// that represents the sequence of <see cref="PathSegment"/> from the root (first element in the list) to the last path segment of the stable URI path 
        /// (last element in the list) combined with the matching stable URI path segment. For instance, the method returns the following list for the passed stable URI path 
        /// '/user/account/4': '{{API-Root,}{user,user},{account,account},{{id},4}}. The method returns an empty list, if the passed stable URI path cannot be mapped.
        /// </summary>
        /// <returns></returns>
        public IList<KeyValuePair<PathSegment,string>> GetPathSegmentsByStableUriPath(string stableUriPath)
        {
            IList<KeyValuePair<PathSegment, string>> pathSegments = new List<KeyValuePair<PathSegment, string>>();

            //split path into path segments
            string[] segments = stableUriPath.Split('/');

            //start at root
            PathSegment currentPathSegment = this.Root;
            //put root as first element to the path Segments list
            pathSegments.Add(new KeyValuePair<PathSegment, string>(this.Root, ""));


            for(int i = 0; i < segments.Length; i++)
            {
                string currentSegment = segments[i];

                //check whether segment is not empty (otherwise skip this segment)
                if (!String.IsNullOrWhiteSpace(currentSegment))
                {
                    bool pathSegmentFound = false;

                    //check static path segments first:
                    for(int j = 0; j < currentPathSegment.ChildrenCount && !pathSegmentFound; j++)
                    {
                        PathSegment pathSegmentChild = (PathSegment)currentPathSegment.Children[j];

                        //if static child matches current segment...
                        if (!pathSegmentChild.IsVariable && pathSegmentChild.MatchesStablePathSegment(currentSegment))
                        {
                            //...add it to the list
                            pathSegments.Add(new KeyValuePair<PathSegment, string>(pathSegmentChild, currentSegment));

                            //...and set flag to found
                            pathSegmentFound = true;

                            //...and set matching path segment to current path segment
                            currentPathSegment = pathSegmentChild;
                        }
                    }
                    //check variable path segments:
                    for(int j = 0; j < currentPathSegment.ChildrenCount && !pathSegmentFound; j++)
                    {
                        PathSegment pathSegmentChild = (PathSegment)currentPathSegment.Children[j];

                        //if variable child matches current segment... (NOTE: We take the first occurence!!)
                        if (pathSegmentChild.IsVariable && pathSegmentChild.MatchesStablePathSegment(currentSegment))
                        {
                            //...add it to the list
                            pathSegments.Add(new KeyValuePair<PathSegment, string>(pathSegmentChild, currentSegment));

                            //...and set flag to found
                            pathSegmentFound = true;

                            //...and set matching path segment to current path segment
                            currentPathSegment = pathSegmentChild;
                        }
                    }

                    //if a segment cannot be mapped...
                    if (!pathSegmentFound)
                    {
                        //...return empty list
                        return new List<KeyValuePair<PathSegment,string>>();
                    }
                }
            }
            return pathSegments;
        }

        /// <summary>
        /// The method maps the passed stable URI path to the underlying <see cref="UriModel"/> and returns the last <see cref="PathSegment"/> of the last path segment of the passed stable URI path as well as the last stable path segment.
        /// The method returns an empty <see cref="KeyValuePair{TKey, TValue}"/>, if the passed stable URI path cannot be mapped.
        /// </summary>
        /// <param name="stableUriPath"></param>
        /// <returns></returns>
        public KeyValuePair<PathSegment,string> GetLastPathSegmentByStableUriPath(string stableUriPath)
        {
            IList<KeyValuePair<PathSegment, string>> pathSegments = GetPathSegmentsByStableUriPath(stableUriPath);
            if(pathSegments.Count > 0)
            {
                return pathSegments[pathSegments.Count - 1];
            }
            else
            {
                return new KeyValuePair<PathSegment, string>();
            }
        }

        /// <summary>
        /// The method calculates the tree path from the API Root to the passed <see cref="PathSegment"/> and tries to map the passed stable path segments to these calculated <see cref="PathSegment"/>s.
        /// Note that only stable path segments are required in the passed dictionary, but anyway, if the dictionary contains a stable path segment of a static <see cref="PathSegment"/>, the method will prefer this value instead of the static value of the <see cref="PathSegment"/>.
        /// The method returns a list of <see cref="KeyValuePair{TKey, TValue}"/>s that represents the sequence of <see cref="PathSegment"/> from the API Root (first element in the list) to the last path segment of the stable URI path 
        /// (last element in the list) combined with the matching stable URI path segment. 
        /// The method returns an empty list, if a stable path segment is required for substitung a variable <see cref="PathSegment"/>, but is missing in the passed dictionary.
        /// </summary>
        /// <param name="lastPathSegment"></param>
        /// <param name="stablePathSegmentValues"></param>
        /// <returns></returns>
        public IList<KeyValuePair<PathSegment,string>> GetPathSegmentsByStablePathSegments(PathSegment lastPathSegment, IDictionary<PathSegment, string> stablePathSegmentValues)
        {
            IList<KeyValuePair<PathSegment, string>> pathSegmentValues = new List<KeyValuePair<PathSegment, string>>();

            //calculate path between root and the passed last path segment
            IList<PathSegment> pathSegments = GetPathSegmentsByLastPathSegment(lastPathSegment);

            foreach(PathSegment pathSegment in pathSegments)
            {
                if (stablePathSegmentValues.ContainsKey(pathSegment))
                {
                    pathSegmentValues.Add(new KeyValuePair<PathSegment, string>(pathSegment, stablePathSegmentValues[pathSegment]));
                }
                else if (pathSegment.IsRoot)
                {
                    pathSegmentValues.Add(new KeyValuePair<PathSegment, string>(pathSegment, ""));
                }
                else if(!pathSegment.IsVariable)
                {
                    pathSegmentValues.Add(new KeyValuePair<PathSegment, string>(pathSegment, pathSegment.Value));
                }
                else
                {
                    //... return empty list
                    return new List<KeyValuePair<PathSegment, string>>();
                }
            }
            return pathSegmentValues;
        }

        /// <summary>
        /// Combines the passed path segment values to a full stable path. Whitespaces in the passed path segment list will be filtered out.
        /// The full path starts always with a leading '/'.
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public static string BuildFullPath(IList<KeyValuePair<PathSegment,string>> segments)
        {
            string[] segmentsAsStringArray = new string[segments.Count];
            for(int i = 0; i < segments.Count; i++)
            {
                segmentsAsStringArray[i] = segments[i].Value;
            }
            return BuildFullPath(segmentsAsStringArray);
        }

        /// <summary>
        /// Combines the passed segments to a full path. Whitespaces in the passed segment array will be filtered out.
        /// The full path starts always with a leading '/'. If the passed segment array contains only one empty segment, '/' representing the API root is returned.
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public static string BuildFullPath(string[] segments)
        {
            //special case API Root, if segments = { "" }
            if(segments.Length == 1 && String.IsNullOrEmpty(segments[0]))
            {
                return "/";
            }

            string fullPath = "";
            for (int i = 0; i < segments.Length; i++)
            {
                if (String.IsNullOrWhiteSpace(segments[i]))
                {
                    continue;
                }
                else
                {
                    fullPath += "/" + segments[i];
                }
            }
            return fullPath;
        }

        /// <summary>
        /// Combines the passed segments up to the specified index to a full path. Whitespaces in the passed segment array will be filtered out.
        /// The full path starts always with a leading '/'. If the passed segment array contains only one empty segment, '/' representing the API root is returned.
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string BuildFullPath(string[] segments, int index)
        {
            //special case API Root, if segments = { "" }
            if (segments.Length == 1 && String.IsNullOrEmpty(segments[0]))
            {
                return "/";
            }

            string fullPath = "";
            for (int i = 0; i < index; i++)
            {
                if (String.IsNullOrWhiteSpace(segments[i]))
                {
                    continue;
                }
                else
                {
                    fullPath += "/" + segments[i];
                }
            }
            return fullPath;
        }

        public override string ToString()
        {
            return Root.ToString(0, new List<int>(), true);
        }
    }
}
