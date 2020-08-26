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

namespace skotstein.net.http.urimodel
{
    /// <summary>
    /// Abstract base class for all tree nodes of the URI Model. Each node contains a <see cref="Value"/>
    /// </summary>
    /// <typeparam name="V">generic value contained within the node</typeparam>
    public abstract class Node<V>
    {
        //The generic value contained within this node
        private V _value;

        //The parent can be either a Root or a Branch Node
        private Node<V> _parentNode;

        /// <summary>
        /// Gets or sets the value of this node
        /// </summary>
        public V Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        public virtual Node<V> Parent
        {
            get
            {
                return _parentNode;
            }

            internal set
            {
                _parentNode = value;
            }
        }

        /// <summary>
        /// Initializes an instance of this class
        /// </summary>
        public Node()
        {

        }


        /// <summary>
        /// Initializes an instance of this class having the passed value
        /// </summary>
        /// <param name="value"></param>
        public Node(V value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns true, if this node is a descendant of the specified target node. A descendant is a node that is reachable by repeated proceeding from a parent node to a child node.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsDescendantOf(Node<V> target)
        {
            if(_parentNode == null)
            {
                //abort if parent is null (negative result)
                return false;
            }
            else if (_parentNode.Equals(target))
            {
                //abort if parent matches the wanted node (positive result)
                return true;
            }
            else
            {
                //recursively ascend to parent node
                return _parentNode.IsDescendantOf(target);
            }
        }

        /// <summary>
        /// Returns true, if this node is a child of the specified target node. A child is a node that is directly connected to a parent and is one of its immediate descendants.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsChildOf(Node<V> target)
        {
            if(_parentNode == null)
            {
                return false;
            }
            else
            {
                return _parentNode.Equals(target);
            }
        }

        /// <summary>
        /// Returns true, if this node is an ancestor of the specified target node. An ancestor is a node that is reachable by repeated proceeding from a child node to a parent node.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public abstract bool IsAncestorOf(Node<V> target);

        /// <summary>
        /// Returns true, if this node is a parent (i.e. an immediate ancestor) of the specified target node. A parent node is directly connected to a child node and is the immediate ancestor of this child node.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public abstract bool IsParentOf(Node<V> target);

        /// <summary>
        /// Gets the number of edges between this node and the root node
        /// </summary>
        public int Depth
        {
            get
            {
                int depth = 0;
                Node<V> parent = Parent;
                while (parent != null)
                {
                    depth++;
                    parent = parent.Parent;
                }
                return depth;
            }
        }

        /// <summary>
        /// Gets the level of this node which is per definition <see cref="Depth"/> + 1
        /// </summary>
        public int Level
        {
            get
            {
                return Depth + 1;
            }
        }

        /// <summary>
        /// Gets the number of edges on the longest path between this node and a descendant leaf (i.e. the number of edges between this node and the deepest leaf in the sub tree).
        /// </summary>
        public abstract int Height { get; }

        /// <summary>
        /// Calculates and returns the the number of edges between this node and the specified target node. If there is no path between these two nodes, the method will return '-1' as result.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public int Distance(Node<V> target)
        {
            return Distance(target, new HashSet<Node<V>>());
        }

        /// <summary>
        /// Calculates and returns the the number of edges between this node and the specified target node. If there is no path between these two nodes, the method will return '-1' as result.
        /// This method is called by <see cref="Distance(Node{V})"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="traversed">the set of traversed nodes</param>
        /// <returns></returns>
        public abstract int Distance(Node<V> target, ISet<Node<V>> traversed);

        /// <summary>
        /// Calculates and returns the path between this node and the specified target node. The returned path is a <see cref="Stack{T}"/> containing all nodes on the path between this node (top element on the stack) and the target node.
        /// If there is no path between this node and the specified target node, the stack will be empty.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Stack<Node<V>> Path(Node<V> target)
        {
            Stack<Node<V>> path = new Stack<Node<V>>();
            Path(target, new HashSet<Node<V>>(), path);
            return path;
        }

        /// <summary>
        /// The method traverses the sub tree of this node (i.e. all descendants and this node) recursively and checks whether the sub tree contains the specified node.
        /// If the sub tree contains the specified node, the method will return true. Moreover, the passed Stack (third argument) contains all nodes on the path from this node (top element on the stack) and the target node.
        /// This method is called by <see cref="Path(Node{V})"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="traversed"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public abstract bool Path(Node<V> target, ISet<Node<V>> traversed, Stack<Node<V>> path);

    }
}
