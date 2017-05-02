# SEPValidationEngine #

A C# micro library for validating properties of arbitrary data

## Installation ##

Clone this repository and add the project to your solution, or use [nuget](https://www.nuget.org/packages/SEP.ValidationEngine/).

## Usage ##

This framework is designed to be used as follows:
 * Subclass from [`BaseValidator`](ValidationEngine/BaseValidator.cs) to create your validator.
 * Subclass from [`ValidatorTestsBase`](ValidationEngine/Test/ValidatorTestsBase.cs) to create a unit test class for your validator.

### Validating a single field ###

To validate that some field conforms to a set of business rules, use an instance of the [`FieldValidations`](ValidationEngine/FieldValidations.cs) class. For example, the following code validates that our data contains a string property, "name", that must be at least 3 characters long.

    public ValidationStatus ValidateName(dynamic data)
    {
      return new FieldValidations<string>("name")
        .ThatIt(IsNotNull<string>())
        .ThatIt(HasNoFewerCharactersThan(3))
        .RunAgainst(() => data.name);
    }

### Validating an object ###

To aggregate the validation results of multiple fields, use the static method `MergeAll` from [`ValidationStatus`](ValidationEngine/ValidationStatus.cs).

    public ValidationStatus Validate(dynamic data)
    {
      return ValidationStatus.MergeAll(
        ValidateName(data),
        ValidateList(data),
        ForEach(() => data.list, ValidateListItem);
      );
    }

Note the use of the ForEach helper from `BaseValidator` to validate each element of the list.

### Validating a collection ###

To validate a collection, break down the problem into

 1. the validation of the collection as a whole and
 2. the validation of each item in the collection

It's best practice to isolate each of these in their own method:

    public ValidationStatus ValidateList(dynamic data)
    {
      return new FieldValidations<IEnumerable<dynamic>>("list")
        .ThatIt(IsNotNull<IEnumerable<dynamic>>())
        .ThatIt(HasNoFewerItemsThan<dynamic>(1, "thing"))
        .ThatIt(HasNoDuplicate<dynamic>("types", (item) => item.type))
        .RunAgainst(() => data.list);
    }

    public ValidationStatus ValidateListItem(dynamic item, int index)
    {
      return new FieldValidations<string>($"list[{index}]")
        .ThatIt(IsNotNull<string>())
        .ThatIt(HasNoFewerCharactersThan(1))
        .RunAgainst(() => data.name);
    }
