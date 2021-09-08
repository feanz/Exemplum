﻿namespace Domain.UnitTests.Todo
{
    using Domain.Todo;
    using FluentAssertions;
    using System;
    using System.Data.Common;
    using Xunit;

    public class ColourTests
    {
        [Fact]
        public void From_for_valid_colour_resolves_colour()
        {
            const string whiteCode = "#FFFFFF";
            
            var sut = Colour.From(whiteCode);
            
            sut.Code.Should().Be(whiteCode);
        }
        
        [Fact]
        public void From_for_invalid_colour_should_throw()
        {
            Action act = () => Colour.From("Foo");
            act.Should().Throw<UnsupportedColourException>();
        }
    }
}