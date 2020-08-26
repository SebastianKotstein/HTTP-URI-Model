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
    /// An instance of this class represents an operation that a client can apply on an URI path. This <see cref="Operation"/> is attached to the last
    /// <see cref="PathSegment"/> of this URI path.
    /// </summary>
    public class Operation
    {
        private PathSegment _pathSegment;
        private HttpMethod _method;

        /// <summary>
        /// Gets or sets the <see cref="PathSegment"/> to which this operation is attached.
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
        /// Gets or sets the <see cref="HttpMethod"/> of this operation.
        /// </summary>
        public HttpMethod Method
        {
            get
            {
                return _method;
            }

            set
            {
                _method = value;
            }
        }

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        /// <param name="pathSegment">the path segment to which this operation is attached</param>
        /// <param name="method"></param>
        public Operation(PathSegment pathSegment, HttpMethod method)
        {
            _pathSegment = pathSegment;
            _method = method;
        }

        /// <summary>
        /// Returns a string representation of this <see cref="Operation"/>
        /// </summary>
        /// <param name="space">number of leading whitespaces and positions for pipe symbols</param>
        /// <param name="pipe">position of pipe symbols</param>
        /// <param name="lastChild">flag inidicating whether this node is the last node in the list of child nodes of the parent node</param>
        public string ToString(int space, IList<int> pipe, bool lastChild)
        {
            string s = "";
            for (int i = 0; i < space; i++)
            {
                if (pipe.Contains(i))
                {
                    s += "│";
                }
                else
                {
                    s += " ";
                }
            }
            if (lastChild)
            {
                s += "└─";
            }
            else
            {
                s += "├─";
            }
            s += Method.ToString() + "\n";
            return s;
        }
    }

    public enum HttpMethod
    {
        GET, PUT, POST, DELETE, PATCH, HEAD, OPTIONS, TRACE
    }
}
