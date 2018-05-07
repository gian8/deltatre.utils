﻿using System;
using System.Linq;
using Deltatre.Utils.Dto;
using NUnit.Framework;

namespace Deltatre.Utils.Tests.Dto
{
	[TestFixture]
	public class ParsingResultWithErrorsTest
	{
		[Test]
		public void CreateInvalid_Throws_When_Errros_Is_Null()
		{
			// ACT
			Assert.Throws<ArgumentNullException>(() => ParsingResult<string, string>.CreateInvalid(null));
		}

		[Test]
		public void CreateInvalid_Allows_To_Create_An_Invalid_Result_With_Errors()
		{
			// ARRANGE
			var errors = new[] { "something bad occurred", "invalid prefix detected" };

			// ACT
			var result = ParsingResult<string, string>.CreateInvalid(errors);

			// ASSERT
			Assert.IsNotNull(result);
			Assert.IsFalse(result.IsValid);
			Assert.IsNull(result.ParsedValue);
			Assert.IsNotNull(result.Errors);
			CollectionAssert.AreEqual(errors, result.Errors);
		}

		[Test]
		public void CreateInvalid_Allows_To_Create_An_Invalid_Result_Without_Specifying_Any_Error()
		{
			// ACT
			var result = ParsingResult<string, string>.CreateInvalid(Enumerable.Empty<string>());

			// ASSERT
			Assert.IsNotNull(result);
			Assert.IsFalse(result.IsValid);
			Assert.IsNull(result.ParsedValue);
			Assert.IsNotNull(result.Errors);
			Assert.IsEmpty(result.Errors);
		}

		[Test]
		public void CreateValid_Creates_Valid_Result()
		{
			// ACT
			var result = ParsingResult<string, string>.CreateValid("my parsed value");

			// ASSERT
			Assert.IsNotNull(result);
			Assert.IsTrue(result.IsValid);
			Assert.AreEqual("my parsed value", result.ParsedValue);
			Assert.IsNotNull(result.Errors);
			Assert.IsEmpty(result.Errors);
		}
	}
}