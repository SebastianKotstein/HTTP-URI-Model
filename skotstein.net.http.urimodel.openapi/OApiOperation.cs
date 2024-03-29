﻿// MIT License
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skotstein.net.http.urimodel.openapi
{
    /// <summary>
    /// <see cref="OApiOperation"/> adds an <see cref="Microsoft.OpenApi.Models.OpenApiOperation"/> property to the original <see cref="Operation"/> type.
    /// </summary>
    public class OApiOperation : Operation
    {
        public static readonly string TEST = "REST";

        private OpenApiOperation _openApiOperation;

        /// <summary>
        /// Gets the nested <see cref="OpenApiOperation"/>.
        /// </summary>
        public OpenApiOperation OpenApiOperation
        {
            get
            {
                return _openApiOperation;
            }
        }

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        /// <param name="pathSegment"></param>
        /// <param name="method"></param>
        /// <param name="operation"></param>
        public OApiOperation(PathSegment pathSegment, HttpMethod method, OpenApiOperation operation) : base(pathSegment, method)
        {
            _openApiOperation = operation;
        }

      
    }
}
