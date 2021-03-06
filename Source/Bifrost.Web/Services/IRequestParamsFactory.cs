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

using System;
using System.Web;

namespace Bifrost.Web.Services
{
    /// <summary>
    /// Defines a factory for building a <see cref="RequestParams">Request Parameters collection </see>
    /// </summary>
    public interface IRequestParamsFactory
    {
        /// <summary>
        /// Builds an instance of <see cref="RequestParams">RequestParams</see> that encapsulates request parameters
        /// </summary>
        /// <param name="request">An HttpRequestBase instance</param>
        /// <returns></returns>
        RequestParams BuildParamsCollectionFrom(HttpRequestBase request);
    }
}