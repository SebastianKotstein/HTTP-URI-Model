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
    /// Helper class that implements methods for calculating metrics on the basis of an embedded <see cref="PathSegment"/>.
    /// Use <see cref="PathSegment.Metrics"/> to obtain an instance of this class.
    /// </summary>
    public class PathSegmentMetrics
    {
        private PathSegment _pathSegment;

        /// <summary>
        /// Initializes an instance of this class for calculating metrics on the basis of the passed <see cref="PathSegment"/>.
        /// </summary>
        /// <param name="pathSegment"></param>
        internal PathSegmentMetrics(PathSegment pathSegment)
        {
            _pathSegment = pathSegment;
        }


        /// <summary>
        /// Returns the number of edges on the longest path between this <see cref="PathSegment"/> and a descendant <see cref="PathSegment"/>
        /// (i.e. the number of edges between this node and the deepest node in the sub tree).
        /// </summary>
        public int PathHeight
        {
            get
            {
                if(_pathSegment.Children.Count == 0)
                {
                    return 0;
                }
                else
                {
                    int maxHeightOfChild = 0;
                    foreach(Node<string> c in _pathSegment.Children)
                    {
                        PathSegment child = (PathSegment)c;
                        int heightOfChild = child.Metrics.PathHeight;
                        if(heightOfChild > maxHeightOfChild)
                        {
                            maxHeightOfChild = heightOfChild;
                        }
                    }
                    return maxHeightOfChild + 1;
                }
            }
        }

        /// <summary>
        /// Returns the number of <see cref="PathParameter"/> of the sub tree (this <see cref="PathSegment"/> is included).
        /// </summary>
        /// <returns></returns>
        public int NumberOfPathParameters()
        {
            int pathParameterCounter = 0;
            IList<PathSegment> variablePathSegments = _pathSegment.QuerySubTree.IsVariable().Results;
            foreach(PathSegment variablePathSegment in variablePathSegments)
            {
                pathParameterCounter += variablePathSegment.PathParameters.Count;
            }
            return pathParameterCounter;
        }
    }
}
