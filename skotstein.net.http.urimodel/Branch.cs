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
    /// Abstract base class for a tree node of the URI Model. A Branch has one parent <see cref="Node{V}"/> and multiple child <see cref="Node{V}"/>s
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public abstract class Branch<V> : Node<V>
    {

        private IList<Node<V>> _children = new List<Node<V>>();

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        public Branch() : base()
        {

        }

        /// <summary>
        /// Initializes an instance of this class having the passed value and parent.
        /// </summary>
        /// <param name="value"></param>
        public Branch(V value) : base(value)
        {
        }

        /// <summary>
        /// Removes all children and unset this node as the parent node of this children. 
        /// </summary>
        public void ClearChildren()
        {
            foreach(Node<V> child in _children)
            {
                child.Parent = null;
            }   
            _children.Clear();
        }

        /// <summary>
        /// Adds a child to this node and sets this node as parent to this child.
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(Node<V> child)
        {
            child.Parent = this;
            _children.Add(child);
        }

        /// <summary>
        /// Removes the passed child node from the list of children and unset this node as the parent of this child.
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChild(Node<V> child)
        {
            child.Parent = null;
            _children.Remove(child);
        }

        /// <summary>
        /// Removes the child node at the specified index position and unset this node as the parent of this child.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveChildAt(int index)
        {
            Node<V> child = _children[index];
            child.Parent = null;
            _children.RemoveAt(index);
        }

        /// <summary>
        /// Returns the child node as the specified index position.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Node<V> GetChild(int index)
        {
            return _children[index];
        }

        /// <summary>
        /// Returns true, if there is a child node at the specified index posotion.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool HasChild(int index)
        {
            return index >= 0 && index < _children.Count;
        }

        /// <summary>
        /// Returns true, if there is a child node that has the specified value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool HasChildWithValue(V value)
        {
            foreach(Node<V> child in _children)
            {
                if (child.Value.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the first occurence of a child node that has the specified value.
        /// The method returns null, if there is no child node that has the specified value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Node<V> GetChildWithValue(V value)
        {
            foreach(Node<V> child in _children)
            {
                if (child.Value.Equals(value))
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns true, if this node has children.
        /// </summary>
        public bool HasChildren
        {
            get
            {
                return _children.Count > 0;
            }
        }

        /// <summary>
        /// Returns the number of children.
        /// </summary>
        public int ChildrenCount
        {
            get
            {
                return _children.Count;
            }
        }

        /// <summary>
        /// Returns a copy of the list of children.
        /// </summary>
        public IList<Node<V>> Children
        {
            get
            {
                IList<Node<V>> copy = new List<Node<V>>();
                foreach(Node<V> child in _children)
                {
                    copy.Add(child);
                }
                return copy;
            }
        }

        public override int Height
        {
            get
            {
                if (!HasChildren)
                {
                    //this node is a leaf
                    return 0;
                }
                else
                {
                    int max = 0;
                    foreach(Node<V> child in _children)
                    {
                        if(child.Height > max)
                        {
                            max = child.Height;
                        }
                    }
                    return max + 1;
                }
            }
        }

        public override bool IsAncestorOf(Node<V> target)
        {
            if (_children.Contains(target))
            {
                return true;
            }
            foreach (Node<V> child in _children)
            {
                if (child.IsAncestorOf(target))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool IsParentOf(Node<V> target)
        {
            return _children.Contains(target);
        }

        public override int Distance(Node<V> target, ISet<Node<V>> traversed)
        {
            /*
             * NOTE: There is either one specific or no path between two nodes in a tree. The distance of this path can be either
             * the number of edges on this path or '-1' if there is no path. As conclusion, it is not necessary to check whether the determined path and, similarily,
             * the distance is the shortest. Every node is only traversed at most once.
             */

            if (traversed.Contains(this))
            {
                return -1;
            }
            traversed.Add(this);

            if (this.Equals(target))
            {
                return 0;
            }

            //check parent node as next hop first
            if(this.Parent != null)
            {
                int distance = this.Parent.Distance(target, traversed);
                if(distance != -1)
                {
                    return distance + 1;
                }
            }

            //then check all children as next hops
            foreach(Node<V> child in _children)
            {
                int distance = child.Distance(target, traversed);
                if(distance != -1)
                {
                    return distance + 1;
                }
            }
      
            return -1;
        }

        public override bool Path(Node<V> target, ISet<Node<V>> traversed, Stack<Node<V>> path)
        {
            if (traversed.Contains(this))
            {
                return false;
            }
            traversed.Add(this);

            if (this.Equals(target))
            {
                path.Push(this);
                return true;
            }

            if(this.Parent != null)
            {
                if (this.Parent.Path(target, traversed, path))
                {
                    path.Push(this);
                    return true;
                }
            }

            foreach(Node<V> child in _children)
            {
                if (child.Path(target, traversed, path))
                {
                    path.Push(this);
                    return true;
                }
            }
            return false;
        }

    }
}
