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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace skotstein.net.http.urimodel
{
    /// <summary>
    /// An instance of this class represents a single path segment of an URI path. 
    /// A <see cref="PathSegment"/> can be either static or variable, which is indicated by the <see cref="IsVariable"/> flag.
    /// A <see cref="PathSegment"/>, which has at least one attached <see cref="Operation"/> represents one or a set of resources. More precisely,
    /// it represents exactely one resource, if the URI path (i.e. the tree path from the <see cref="ApiRoot"/> to this <see cref="PathSegment"/>) consists
    /// only of static <see cref="PathSegment"/>s. If the URI path contains at least one variable <see cref="PathSegment"/>, then it represents a set of resources.
    /// </summary>
    public class PathSegment : Branch<string>, IAllocatable
    {
        private string _uriPath;
        private IList<PathParameter> _pathParameters = new List<PathParameter>();
        private IList<Operation> _operations = new List<Operation>();

        private PathSegmentMetrics _metrics;

        private bool _isAllocated;

        private string _indexedValue;

        /// <summary>
        /// Gets the full URI path that is composed of all <see cref="PathSegment"/>s.
        /// Note that the allocation of <see cref="PathParameter"/>s does not affect this value.
        /// </summary>
        public string UriPath
        {
            get
            {
                return _uriPath;
            }
        }

        /// <summary>
        /// Returns true, if this <see cref="PathSegment"/> is variable, which means that it has at least one <see cref="PathParameter"/>. 
        /// Otherwise, i.e. for a static <see cref="PathSegment"/>, this property is false. Note that this property is false, if this <see cref="PathSegment"/>
        /// has at least one <see cref="PathParameter"/>, but is currently allocated (check <see cref="IsAllocated"/>).
        /// </summary>
        public bool IsVariable
        {
            get
            {
                if (HasPathParameters)
                {
                    if (IsAllocated)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns true, if this <see cref="PathSegment"/> has <see cref="PathParameters"/>.
        /// </summary>
        public bool HasPathParameters
        {
            get
            {
                return _pathParameters.Count > 0;
            }
        }

        /// <summary>
        /// Gets the list of <see cref="PathParameter"/>s.
        /// </summary>
        public IList<PathParameter> PathParameters
        {
            get
            {
                return _pathParameters;
            }
        }

        /// <summary>
        /// Gets the list of <see cref="Operation"/>s.
        /// </summary>
        public IList<Operation> Operations
        {
            get
            {
                return _operations;
            }
        }

        /// <summary>
        /// Returns true, if this <see cref="PathSegment"/> has no <see cref="Parent"/> (i.e. <see cref="Parent"/> is null).
        /// </summary>
        public bool IsRoot
        {
            get
            {
                return Parent == null;
            }
        }

        /// <summary>
        /// Gets the regular expression (<see cref="Regex"/>) for matching a stable URI path segment against this <see cref="PathSegment"/>.
        /// </summary>
        public Regex Regex
        {
            get
            {
                //create regex for identifying path parameter
                Regex pathParameterRegex = new Regex(@"(\{.*?\})");

                //substitute all path parameter with '$' placeholder, e.g.: a{x}:{y}b --> a$:$b
                string modifiedValue = pathParameterRegex.Replace(Value, "$");

                //split path segment value by '$' placeholder
                string[] staticElements = modifiedValue.Split('$');

                //create pattern:
                string pattern = "";
                for (int i = 0; i < staticElements.Length; i++)
                {

                    //for each '$' placeholder (= path parameter):
                    if (i > 0)
                    {
                        //add path parameter regex: any symbol '.' at least one time '+'
                        pattern += @"(.+)";
                    }

                    //for each static content (= content that is not encapsulated within brackets)
                    if (!String.IsNullOrWhiteSpace(staticElements[i]))
                    {
                        
                        string modifiedStaticElement = "";
                        foreach (char c in staticElements[i])
                        {
                            if (char.IsLetterOrDigit(c))
                            {
                                modifiedStaticElement += c;
                            }
                            else
                            {
                                //add a slash to special characters
                                modifiedStaticElement += @"\" + c;
                            }
                        }

                        //add static content regex
                        pattern += @"(?:(" + modifiedStaticElement.ToLower() + "))";
                    }
                }

                //add start and end to regex
                pattern = "^(" + pattern + ")$";

                return new Regex(pattern);
            }
        }
         
        /// <summary>
        /// Returns true, if this <see cref="PathSegment"/> has at least one <see cref="Operation"/> attached and, therefore, represents either one or multiple resources.
        /// </summary>
        public bool HasOperations
        {
            get
            {
                return _operations.Count > 0;
            }
        }

        public bool IsAllocated
        {
            get
            {
                return _isAllocated;
            }
            set
            {
                _isAllocated = value;
            }
        }

        /// <summary>
        /// Gets the next ancestor of this <see cref="PathSegment"/> that supports at least one operation.
        /// The property returns null, if no such next ancestor for this <see cref="PathSegment"/> exists.
        /// Refer to <see cref="IsNextAncestorWithOperationsOf(PathSegment)"/> for the definition of a next ancestor.
        /// </summary>
        public PathSegment NextAncestorWithOperations
        {
            get
            {
                //search for the next ancestor by ascending starting at the parent of this path segment
                PathSegment p = (PathSegment)this.Parent;
                while (p != null)
                {
                    if (p.HasOperations)
                    {
                        //if the parent supports at least one operation, than it is the next ancestor with operations
                        return p;
                    }
                    //otherwise continue to ascent
                    p = (PathSegment)p.Parent;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the list containing all next descendants of this <see cref="PathSegment"/> that support at least one operation.
        /// The list is empty, if no such next descendant for this <see cref="PathSegment"/> exists.
        /// Refer to see <see cref="IsNextDescendantWithOperationsOf(PathSegment)"/> for the definition of a next descendant.
        /// </summary>
        public IList<PathSegment> NextDescendantsWithOperations
        {
            get
            {
                IList<PathSegment> nextDescendants = new List<PathSegment>();
                foreach (Node<string> c in Children)
                {
                    PathSegment child = (PathSegment)c;
                    if (child.HasOperations)
                    {
                        nextDescendants.Add(child);
                    }
                    else
                    {
                        IList<PathSegment> nextDescendantsOfChild = child.NextDescendantsWithOperations;
                        foreach (PathSegment nextDescendantOfChild in nextDescendantsOfChild)
                        {
                            nextDescendants.Add(nextDescendantOfChild);
                        }
                    }
                }
                return nextDescendants;
            }
        }

        /// <summary>
        /// Returns a list containing all <see cref="PathSegment"/>s of the sub tree of this <see cref="PathSegment"/>.
        /// Note that this <see cref="PathSegment"/> is part of the sub tree and, therefore, contained within the returned list.
        /// Use <see cref="QuerySubTree"/> instead, if you want to filter the returned list.
        /// </summary>
        public IList<PathSegment> SubTree
        {
            get
            {
                IList<PathSegment> subTree = new List<PathSegment>();
                subTree.Add(this);
                foreach (Node<string> c in Children)
                {
                    PathSegment child = (PathSegment)c;
                    IList<PathSegment> subTreeOfChild = child.SubTree;
                    foreach (PathSegment childInSubTreeOfChild in subTreeOfChild)
                    {
                        subTree.Add(childInSubTreeOfChild);
                    }
                }
                return subTree;
            }
        }

        /// <summary>
        /// Returns a <see cref="PathSegmentQuery"/> object containing all <see cref="PathSegment"/>s of the sub tree of this this <see cref="PathSegment"/>.
        /// Note that this <see cref="PathSegment"/> is part of the sub tree and, therefore, contained within the returned object.
        /// </summary>
        public PathSegmentQuery QuerySubTree
        {
            get
            {
                return new PathSegmentQuery(SubTree);
            }
        }

        /// <summary>
        /// Returns the <see cref="PathSegmentMetrics"/> object of this <see cref="PathSegment"/> in order to calculate metrics.
        /// </summary>
        public PathSegmentMetrics Metrics
        {
            get
            {
                return _metrics;
            }
        }

        /// <summary>
        /// Returns the <see cref="ResourceEndpointType"/> of this <see cref="PathSegment"/>.
        /// If this <see cref="PathSegment"/> has no <see cref="Operation"/>s, the method will return <see cref="ResourceEndpointType.none"/>.
        /// If this <see cref="PathSegment"/> has at least one operation and there is at least one variable <see cref="PathSegment"/> on the path from this <see cref="PathSegment"/> to the <see cref="ApiRoot"/>, the method returns <see cref="ResourceEndpointType.multiple"/>.
        /// If this <see cref="PathSegment"/> has at least one operation and there are no variable path <see cref="PathSegment"/>s on the path from this <see cref="PathSegment"/> to the <see cref="ApiRoot"/> or all variable <see cref="PathSegment"/>s are allocated with a specific value, the method will return <see cref="ResourceEndpointType.single"/>.
        /// </summary>
        public ResourceEndpointType ResourceEndpointType
        {
            get
            {
                if (HasOperations)
                {
                    PathSegment current = this;
                    while (current != null)
                    {
                        if (current.IsVariable)
                        {
                            return ResourceEndpointType.multiple;
                        }
                        current = (PathSegment)current.Parent;
                    }
                    return ResourceEndpointType.one;
                }
                else
                {
                    return ResourceEndpointType.none;
                }
            }
        }

        /// <summary>
        /// Gets or sets the indexed value. The indexed value is primarily used for variable <see cref="PathSegment"/> and contains indexes for each path parameter.
        /// More precisely, the placeholder for a respective path parameter is replaced by its index. Use <see cref="PathParameter.ExtractPathParameter(string, out string)"/>
        /// to create this indexed value. Note that this indexed value is pre-initialized with the <see cref="Node{V}.Value"/> of this <see cref="PathSegment"/>.
        /// </summary>
        public string IndexedValue
        {
            get
            {
                return _indexedValue;
            }

            set
            {
                _indexedValue = value;
            }
        }

        /// <summary>
        /// Initializes an instance of this class that has the passed name (i.e. the sequence of characters after a slash, e.g. 'account' for '/user/account').
        /// </summary>
        /// <param name="value"></param>
        /// <param name="uriPath"></param>
        public PathSegment(string value, string uriPath) : base(value)
        {
            _indexedValue = value;
            _uriPath = uriPath;
            _metrics = new PathSegmentMetrics(this);
        }

        public void ResetAllocation()
        {
            this.IsAllocated = false;
            foreach(Node<string> child in Children)
            {
                ((PathSegment)(child)).ResetAllocation();
            }
        }

        /// <summary>
        /// Returns true, if this <see cref="PathSegment"/> supports an <see cref="Operation"/> that has the specified HTTP method.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public bool SupportsOperation(HttpMethod method)
        {
            foreach(Operation operation in Operations)
            {
                if(operation.Method == method)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true, if the passed stable path segment matches this <see cref="PathSegment"/>.
        /// </summary>
        /// <param name="stablePathSegment"></param>
        /// <returns></returns>
        public bool MatchesStablePathSegment(string stablePathSegment)
        {
            foreach(Match match in this.Regex.Matches(stablePathSegment.ToLower()))
            {
                if (match.Value.CompareTo(stablePathSegment.ToLower()) == 0)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Creates a stable URI path segment based on this URI Template <see cref="PathSegment"/> by injecting the passed path parameter values.
        /// If this <see cref="PathSegment"/> is static, use the <see cref="Node{V}.Value"/> property instead to query the static stable URI path.
        /// </summary>
        /// <param name="pathParameterValues"></param>
        /// <returns></returns>
        public string BuildStablePathSegment(IDictionary<PathParameter, string> pathParameterValues)
        {
            string stablePathSegment = IndexedValue;
            foreach(KeyValuePair<PathParameter,string> pathParameterValue in pathParameterValues)
            {
                stablePathSegment = stablePathSegment.Replace("[p" + pathParameterValue.Key.Index + "]", pathParameterValue.Value + "");
            }
            return stablePathSegment;
        }

        /// <summary>
        /// Returns true, if this <see cref="PathSegment"/> is the next ancestor of the passed <see cref="PathSegment"/> that has at least one operation.
        /// This path segment p is the next ancestor of the passed path segment q, iff p has at least one operation (i.e. represents resources), is an ancestor of q, and there is no other ancestor on the path between p and q.
        /// </summary>
        /// <param name="pathSegment"></param>
        /// <returns></returns>
        public bool IsNextAncestorWithOperationsOf(PathSegment pathSegment)
        {
            //let 'pathSegment' = q

            if (this.HasOperations)
            {
                //search for next ancestor by ascending starting at the parent of q
                pathSegment = (PathSegment)pathSegment.Parent;
                while(pathSegment != null)
                {
                    if(pathSegment == this)
                    {
                        //if the first ancestor is equals this path segment (and since this path segment has operations, it is the next ancestor with operations)
                        return true;
                    }
                    else if (pathSegment.HasOperations)
                    {
                        //if the first ancestor has operations but is not this path segment
                        return false;
                    }
                    //otherwise continue to ascent
                    pathSegment = (PathSegment)pathSegment.Parent;
                }
                //when root is reached
                return false;
            }
            else
            {
                //if this path segment does not support any operation, then it cannot be an next ancestor with operations of q
                return false;
            }
        }

        /// <summary>
        /// Returns true, if this <see cref="PathSegment"/> is a next descendant of the passed <see cref="PathSegment"/> that has at least one operation.
        /// This path segment p is a next descendant of the passed path segment q, iff p has at least one operation (i.e. represents resources), is a descendant of q, and there is no other descendant on the path between p and q.
        /// </summary>
        /// <param name="pathSegment"></param>
        /// <returns></returns>
        public bool IsNextDescendantWithOperationsOf(PathSegment pathSegment)
        {
            //let 'pathSegment' = q

            if (this.HasOperations)
            {
                //search for next descendant by ascending starting at the parent of this path segment ps
                PathSegment ps = (PathSegment)this.Parent;
                while(ps != null)
                {
                    if(ps == pathSegment)
                    {
                        //if the first ancestor is equals the passed path segment q, the this path segment p is the next descendant of q
                        return true;
                    }
                    else if (ps.HasOperations)
                    {
                        //if the first ancestor has operations but is not the passed path segment q, then p is the next descendant of this first ancestor, but not of q
                        return false;
                    }
                    //otherwise continue to ascent
                    ps = (PathSegment)ps.Parent;
                }
                //when root is reached
                return false;
            }
            else
            {
                //if this path segment does not support any operation, then it cannot be an next descendant with operations of q
                return false;
            }
        }



        /// <summary>
        /// Returns a string representation of the underlying sub tree
        /// </summary>
        /// <param name="space">number of leading whitespaces and positions for pipe symbols</param>
        /// <param name="pipe">position of pipe symbols</param>
        /// <param name="lastChild">flag inidicating whether this node is the last node in the list of child nodes of the parent node</param>
        /// <returns></returns>
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

            IList<int> copyPipe = new List<int>();
            foreach (int p in pipe)
            {
                copyPipe.Add(p);
            }

            if (lastChild)
            {

                s += "└─/";

                /*
                if (this.Parent == null) //no slash for the root element
                {
                    s += "└─";
                }
                else
                {
                   
                }
                */

            }
            else
            {
                s += "├─/";
                copyPipe.Add(space);
            }
            if (!HasOperations)
            {
                s += Value + "\n";
            }
            else
            {
                s += Value + " (";
                foreach (Operation operation in Operations)
                {
                    s += operation.Method + ",";
                }
                s = s.TrimEnd(',');
                s += ")\n";
            }
            for (int i = 0; i < ChildrenCount; i++)
            {
                bool pLastChild = (i + 1 == ChildrenCount);

                Node<string> child = GetChild(i);
                if (child is PathSegment)
                {
                    s += ((PathSegment)child).ToString(space + 3, copyPipe, pLastChild);
                }
            }
            return s;
        }

    }

    public enum ResourceEndpointType
    {
        none, one, multiple
    }
}
