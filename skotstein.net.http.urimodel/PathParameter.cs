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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace skotstein.net.http.urimodel
{
    /// <summary>
    /// An instance of this class represents a single path parameter of a variable <see cref="urimodel.PathSegment"/>.
    /// </summary>
    public class PathParameter //: IAllocatable<string>
    {
        private string _parameterName;
        private int _index;
        private PathSegment _pathSegment;

        /// <summary>
        /// Gets or sets the <see cref="PathSegment"/> to which this path parameter belongs to.
        /// </summary>
        public PathSegment PathSegment
        {
            get
            {
                return _pathSegment;
            }
            set
            {
                _pathSegment = value;
            }
        }

        /// <summary>
        /// Gets or sets the parameter name of this path parameter.
        /// The parameter name is without brackets (e.g. 'id' instead of '{id}').
        /// </summary>
        public string ParameterName
        {
            get
            {
                return _parameterName;
            }
            set
            {
                _parameterName = value;
            }

        }

        /// <summary>
        /// Gets or sets the index pointing to the position of the opening bracket ('{') of this <see cref="PathSegment"/>.
        /// </summary>
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
            }
        }

        /// <summary>
        /// Returns a list of <see cref="PathParameter"/>s that the method extracts from the passed input string.
        /// A sub string in the input string is considered as an <see cref="PathParameter"/>, if it matches the regex '(\{.*?\})'.
        /// The method returns an empty list, if the input string does not contain sub string that has the path parameter format.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="indexedPathSegmentString"></param>
        /// <returns></returns>
        public static IList<PathParameter> ExtractPathParameter(string input, out string indexedPathSegmentString)
        {
            IList<PathParameter> pathParameters = new List<PathParameter>();

            int index = 0;
            Regex regex = new Regex(@"(\{.*?\})");
            foreach(Match match in regex.Matches(input))
            {
                PathParameter pathParameter = new PathParameter()
                {
                    ParameterName = match.Value.Replace("{", "").Replace("}", ""), //set parameter name WITHOUT brackets
                    Index = index++
                    //Index = match.Index,
                    //Length = match.Length
                };
                pathParameters.Add(pathParameter);
            }

            //create indexPathSegmentString:
            indexedPathSegmentString = CreateIndexedPathSegmentString(input);

            return pathParameters;
        }

        /// <summary>
        /// Converts the input string representing a variable path segment, e.g. 'a{x}:{y}b{y}', into a string that numbers the position
        /// of the <see cref="PathParameter"/>s in the original input string. In detail, the method replaces the respective path parameter placeholder
        /// by an index indicating the number of the respective path parameter in the path segment string. For instance, the output of the input string
        /// 'a{x}:{y}b{y}' is 'a[p0]:[p1]b[p2]'
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string CreateIndexedPathSegmentString(string input)
        {
            Regex regex = new Regex(@"(\{.*?\})");

            int count = regex.Matches(input).Count;
            for (int i = 0; i < count; i++)
            {
                input = regex.Replace(input, "[p" + i + "]", 1);
            }
            return input;
        }

    }
}
