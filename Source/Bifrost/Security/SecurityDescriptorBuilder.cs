﻿#region License
//
// Copyright (c) 2008-2015, Dolittle (http://www.dolittle.com)
//
// Licensed under the MIT License (http://opensource.org/licenses/MIT)
//
// You may not use this file except in compliance with the License.
// You may obtain a copy of the license at 
//
//   http://github.com/dolittle/Bifrost/blob/master/MIT-LICENSE.txt
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion
namespace Bifrost.Security
{
    /// <summary>
    /// Defines a builder for building a <see cref="ISecurityDescriptor"/>
    /// </summary>
    public class SecurityDescriptorBuilder : ISecurityDescriptorBuilder
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SecurityDescriptorBuilder"/>
        /// </summary>
        /// <param name="descriptor">The <see cref="ISecurityDescriptor"/> we are building</param>
        public SecurityDescriptorBuilder(ISecurityDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

#pragma warning disable 1591 // Xml Comments
        public ISecurityDescriptor Descriptor { get; private set; } 
#pragma warning restore 1591 // Xml Comments
    }
}
