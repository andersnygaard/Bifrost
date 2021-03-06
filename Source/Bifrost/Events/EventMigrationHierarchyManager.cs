#region License
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
using System.Collections.Generic;
using System.Linq;
using Bifrost.Execution;

namespace Bifrost.Events
{
    /// <summary>
    /// Represents a <see cref="IEventMigrationHierarchyManager">IEventMigrationHierarchyManager</see>
    /// </summary>
    /// <remarks>
    /// The manager will automatically build an <see cref="EventMigrationHierarchy">EventMigrationHierarchy</see> for all events and
    /// allow clients to query for the current migration level for a specific logical event or the concrete type of a particular link
    /// in the migration chain for a logical event.
    /// </remarks>
    [Singleton]
    public class EventMigrationHierarchyManager : IEventMigrationHierarchyManager
    {
        private readonly IEventMigrationHierarchyDiscoverer _eventMigrationHierarchyDiscoverer;
        private readonly IEnumerable<EventMigrationHierarchy> _hierarchies;

        /// <summary>
        /// Initializes an instance of <see cref="EventMigrationHierarchyManager">EventMigrationHierarchyManager</see>
        /// </summary>
        /// <param name="eventMigrationHierarchyDiscoverer">IEventMigrationHierarchyDiscoverer</param>
        public EventMigrationHierarchyManager(IEventMigrationHierarchyDiscoverer eventMigrationHierarchyDiscoverer)
        {
            _eventMigrationHierarchyDiscoverer = eventMigrationHierarchyDiscoverer;
            _hierarchies = _eventMigrationHierarchyDiscoverer.GetMigrationHierarchies();
        }

#pragma warning disable 1591 // Xml Comments
        public int GetCurrentMigrationLevelForLogicalEvent(Type logicalEvent)
        {
            var hierarchy = GetHierarchyForLogicalType(logicalEvent);
            return hierarchy.MigrationLevel;
        }

        public Type GetConcreteTypeForLogicalEventMigrationLevel(Type logicalEvent, int level)
        {
            if(level < 0)
                throw new MigrationLevelOutOfRangeException(string.Format("The lowest possible migration level is 0.  You asked for {0}",level));


            var hierarchy = GetHierarchyForLogicalType(logicalEvent);
            Type type;
            try
            {
                 type = hierarchy.GetConcreteTypeForLevel(level);
            }
            catch (Exception ex)
            {
                throw new MigrationLevelOutOfRangeException(string.Format(
                                        "The maximum migration level for the logical event {0} is {1}.  Does not have a migration level of {2}",
                                        logicalEvent.FullName, hierarchy.MigrationLevel, level
                                        ),ex);
            }

            return type;
        }

        public Type GetLogicalTypeForEvent(Type @event)
        {
            var hierarchy = _hierarchies.Where(h => h.MigratedTypes.Contains(@event)).FirstOrDefault();

            if(hierarchy == null)
                throw new UnregisteredEventException(string.Format("Cannot find an event migration hierarchy that contains '{0}' event type", @event.AssemblyQualifiedName));

            return hierarchy.LogicalEvent;
        }

        public Type GetLogicalTypeFromName(string typeName)
        {
            var hierarchy = _hierarchies.Where(h => h.LogicalEvent.Name == typeName).FirstOrDefault();

            if (hierarchy == null)
                throw new UnregisteredEventException(string.Format("Cannot find an event migration hierarchy with the logical event named '{0}'.", typeName));

            return hierarchy.LogicalEvent;
        }

#pragma warning restore 1591 // Xml Comments

        private EventMigrationHierarchy GetHierarchyForLogicalType(Type logicalEvent)
        {
            var hierarchy = _hierarchies.Where(hal => hal.LogicalEvent == logicalEvent).FirstOrDefault();

            if(hierarchy == null)
                throw new UnregisteredEventException(string.Format("Cannot find the logical event '{0}' in the migration hierarchies.", logicalEvent.AssemblyQualifiedName));

            return hierarchy;
        }
    }
}