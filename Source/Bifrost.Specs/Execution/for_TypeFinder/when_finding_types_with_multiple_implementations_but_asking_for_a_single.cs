﻿using System;
using Bifrost.Execution;
using Machine.Specifications;

namespace Bifrost.Specs.Execution.for_TypeFinder
{
    [Subject(typeof(TypeFinder))]
    public class when_finding_types_with_multiple_implementations_but_asking_for_a_single : given.a_type_finder
    {
        static Exception exception;

        Establish context = () => contract_to_implementors_map_mock.Setup(c => c.GetImplementorsFor(typeof(IMultiple))).Returns(new[] { typeof(FirstMultiple), typeof(SecondMultiple) });

        Because of = () => exception = Catch.Exception(() => type_finder.FindSingle<IMultiple>(contract_to_implementors_map_mock.Object));

        It should_throw_a_multiple_types_found_exception = () => exception.ShouldBeOfExactType<MultipleTypesFoundException>();
    }
}