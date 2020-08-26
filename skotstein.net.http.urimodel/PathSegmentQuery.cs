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
using System.Collections.Generic;

namespace skotstein.net.http.urimodel
{
    /// <summary>
    /// The class <see cref="PathSegmentQuery"/> offers a fluent interface for querying and filtering <see cref="PathSegment"/>s.
    /// Each method represents a specific filter and takes a <see cref="PathSegmentQuery"/> object as input and creates a new <see cref="PathSegmentQuery"/> object
    /// encapsulating a reduced (i.e. filtered) set of <see cref="PathSegment"/>s. Chaining these filter methods represents a conjunction (logical AND) of these invidual filters.
    /// For more complex logical operations (besides a logical AND), is it possible to integrate <see cref="Not(IPathSegmentFilter)"/> and <see cref="Or(IPathSegmentFilter[])"/> operators.
    /// </summary>
    public class PathSegmentQuery
    {
        private IList<PathSegment> _values = new List<PathSegment>();

        /// <summary>
        /// Gets the result of the query.
        /// </summary>
        public IList<PathSegment> Results
        {
            get
            {
                return _values;
            }
        }

        /// <summary>
        /// Initializes an instance of this class with the specified input set of <see cref="PathSegment"/>s.
        /// </summary>
        /// <param name="inputSet"></param>
        internal PathSegmentQuery(IList<PathSegment> inputSet)
        {
            foreach(PathSegment pathSegement in inputSet)
            {
                _values.Add(pathSegement);
            }
        }

        /// <summary>
        /// Filters the input set based on the passed <see cref="IPathSegmentFilter"/> and creates a new <see cref="PathSegmentQuery"/> object containing the filter set of <see cref="PathSegment"/>s.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private PathSegmentQuery Filter(IPathSegmentFilter filter)
        {
            IList<PathSegment> output = new List<PathSegment>();
            foreach (PathSegment pathSegment in _values)
            {
                if (filter.Filter(pathSegment))
                {
                    output.Add(pathSegment);
                }
            }
            return new PathSegmentQuery(output);
        }

        /// <summary>
        /// Creates a <see cref="PathSegmentQuery"/> object containing only <see cref="PathSegment"/>s that are variable.
        /// </summary>
        /// <returns></returns>
        public PathSegmentQuery IsVariable()
        {
            return Filter(QueryFilter.IsVariable());
        }

        /// <summary>
        /// Creates a <see cref="PathSegmentQuery"/> object containing only <see cref="PathSegment"/>s that are static.
        /// </summary>
        public PathSegmentQuery IsStatic()
        {
            return Filter(QueryFilter.IsStatic());
        }

        /// <summary>
        /// Creates a <see cref="PathSegmentQuery"/> object containing only <see cref="PathSegment"/>s that represent a resource endpoint (see <see cref="ResourceEndpointType"/>).
        /// </summary>
        public PathSegmentQuery IsResourceEndpoint()
        {
            return Filter(QueryFilter.IsResourceEndpoint());
        }

        /// <summary>
        /// Creates a <see cref="PathSegmentQuery"/> object containing only <see cref="PathSegment"/>s that represent the pased resource endpoint type (see <see cref="ResourceEndpointType"/>).
        /// </summary>
        public PathSegmentQuery IsResourceEndpointType(ResourceEndpointType type)
        {
            return Filter(QueryFilter.IsResourceEndpointType(type));
        }

        /// <summary>
        /// Creates a <see cref="PathSegmentQuery"/> object containing only <see cref="PathSegment"/>s that support the passed operationt type (see <see cref="HttpMethod"/>).
        /// </summary>
        public PathSegmentQuery HasOperation(HttpMethod method)
        {
            return Filter(QueryFilter.HasOperation(method));
        }

        /// <summary>
        /// Creates a <see cref="PathSegmentQuery"/> object containing only <see cref="PathSegment"/>s that support at least one operation.
        /// </summary>
        public PathSegmentQuery HasOperations()
        {
            return Filter(QueryFilter.HasOperations());
        }

        /// <summary>
        /// Creates a <see cref="PathSegmentQuery"/> object containing only <see cref="PathSegment"/>s that are filtered by the specified <see cref="IPathSegmentFilter"/> whose result is negated.
        /// </summary>
        public PathSegmentQuery Not(IPathSegmentFilter filter)
        {
            return Filter(QueryFilter.Not(filter));
        }

        /// <summary>
        /// Creates a <see cref="PathSegmentQuery"/> object containing only <see cref="PathSegment"/>s that match all (logical AND) of the specified <see cref="IPathSegmentFilter"/>s.
        /// </summary>
        public PathSegmentQuery And(params IPathSegmentFilter[] filters)
        {
            return Filter(QueryFilter.And(filters));
        }

        /// <summary>
        /// Creates a <see cref="PathSegmentQuery"/> object containing only <see cref="PathSegment"/>s that match at least one (logical OR) of the specified <see cref="IPathSegmentFilter"/>s.
        /// </summary>
        public PathSegmentQuery Or(params IPathSegmentFilter[] filters)
        {
            return Filter(QueryFilter.Or(filters));
        }
    }

    /// <summary>
    /// Base type for all <see cref="PathSegment"/> filter.
    /// </summary>
    public interface IPathSegmentFilter
    {
        /// <summary>
        /// Returns true, if the passed <see cref="PathSegment"/> matches the underlying filter.
        /// </summary>
        /// <param name="pathSegment"></param>
        /// <returns></returns>
        bool Filter(PathSegment pathSegment);
    }

    /// <summary>
    /// Helper class that provides static methods for creating several different filter types
    /// </summary>
    public class QueryFilter
    {
        /// <summary>
        /// Creates and returns a new <see cref="AndFilter"/> instance. 
        /// The returned <see cref="AndFilter"/> represents a conjunction (logical AND) of the passed <see cref="IPathSegmentFilter"/>s.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static IPathSegmentFilter And(params IPathSegmentFilter[] filters)
        {
            return new AndFilter(filters);
        }

        /// <summary>
        /// Creates and returns a new <see cref="OrFilter"/> instance.
        /// The returned <see cref="OrFilter"/> represents a disjunction (logical OR) of the passed <see cref="IPathSegmentFilter"/>s.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static IPathSegmentFilter Or(params IPathSegmentFilter[] filters)
        {
            return new OrFilter(filters);
        }

        /// <summary>
        /// Creates and returns a new <see cref="NotFilter"/> instance.
        /// The returned <see cref="NotFilter"/> negates the passed <see cref="IPathSegmentFilter"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IPathSegmentFilter Not(IPathSegmentFilter filter)
        {
            return new NotFilter(filter);
        }

        /// <summary>
        /// Creates and returns a <see cref="IsVariableFilter"/> instance.
        /// </summary>
        /// <returns></returns>
        public static IPathSegmentFilter IsVariable()
        {
            return new IsVariableFilter();
        }

        /// <summary>
        /// Creates and returns a <see cref="NotFilter"/> nesting (i.e. negating) a <see cref="IsVariableFilter"/>.
        /// </summary>
        /// <returns></returns>
        public static IPathSegmentFilter IsStatic()
        {
            return new NotFilter(new IsVariableFilter());
        }

        /// <summary>
        /// Creates and returns a <see cref="IsResourceEndpointTypeFilter"/> filtering the passed <see cref="ResourceEndpointType"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IPathSegmentFilter IsResourceEndpointType(ResourceEndpointType type)
        {
            return new IsResourceEndpointTypeFilter(type);
        }

        /// <summary>
        /// Creates and returns a <see cref="IPathSegmentFilter"/> that filters <see cref="PathSegment"/> representing resource endpoints.
        /// </summary>
        /// <returns></returns>
        public static IPathSegmentFilter IsResourceEndpoint()
        {
            return new OrFilter(new IsResourceEndpointTypeFilter(ResourceEndpointType.one), new IsResourceEndpointTypeFilter(ResourceEndpointType.multiple));
        }

        /// <summary>
        /// Creates and returns a <see cref="HasOperationFilter"/> filtering all <see cref="PathSegment"/>s that support the passed <see cref="HttpMethod"/>.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static IPathSegmentFilter HasOperation(HttpMethod method)
        {
            return new HasOperationFilter(method);
        }

        /// <summary>
        /// Creates and returns <see cref="HasOperationFilter"/> filtering all <see cref="PathSegment"/> that support at least one operation.
        /// </summary>
        /// <returns></returns>
        public static IPathSegmentFilter HasOperations()
        {
            return new HasOperationsFilter();
        }
    }


    public class AndFilter : IPathSegmentFilter
    {
        private IList<IPathSegmentFilter> _filters = new List<IPathSegmentFilter>();

        public AndFilter(params IPathSegmentFilter[] filters)
        {
            foreach(IPathSegmentFilter filter in filters)
            {
                _filters.Add(filter);
            }
        }

        public bool Filter(PathSegment pathSegment)
        {
            foreach(IPathSegmentFilter filter in _filters)
            {
                if (!filter.Filter(pathSegment))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class OrFilter : IPathSegmentFilter
    {
        private IList<IPathSegmentFilter> _filters = new List<IPathSegmentFilter>();

        public OrFilter(params IPathSegmentFilter[] filters)
        {
            foreach(IPathSegmentFilter filter in _filters)
            {
                _filters.Add(filter);
            }
        }

        public bool Filter(PathSegment pathSegment)
        {
            foreach(IPathSegmentFilter filter in _filters)
            {
                if (filter.Filter(pathSegment))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class NotFilter : IPathSegmentFilter
    {
        private IPathSegmentFilter _filter;

        public NotFilter(IPathSegmentFilter filter)
        {
            _filter = filter;
        }

        public bool Filter(PathSegment pathSegment)
        {
            return !_filter.Filter(pathSegment);
        }
    }

    public class IsVariableFilter : IPathSegmentFilter
    {
        public bool Filter(PathSegment pathSegment)
        {
            return pathSegment.IsVariable;
        }
    }

    public class IsResourceEndpointTypeFilter : IPathSegmentFilter
    {
        private ResourceEndpointType _typeFilter;

        public IsResourceEndpointTypeFilter(ResourceEndpointType type)
        {
            _typeFilter = type;
        }

        public bool Filter(PathSegment pathSegment)
        {
            return pathSegment.ResourceEndpointType == _typeFilter;
        }
    }

    public class HasOperationFilter : IPathSegmentFilter
    {
        private HttpMethod _method;

        public HasOperationFilter(HttpMethod method)
        {
            _method = method;
        }

        public bool Filter(PathSegment pathSegment)
        {
            return pathSegment.SupportsOperation(_method);
        }
    }

    public class HasOperationsFilter : IPathSegmentFilter
    {
        public bool Filter(PathSegment pathSegment)
        {
            return pathSegment.HasOperations;
        }
    }
}
