using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryModelTests.dvhTest;

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; init; }
}        /*

// The `LastName` property can be set only during initialization. It CAN'T be modified afterwards.
// The `FirstName` property can be modified after initialization.
var pet = new Person() { FirstName = "Joe", LastName = "Doe" };

// You can assign the FirstName property to a different value.
pet.FirstName = "Jane";

// Compiler error:
// Error CS8852  Init - only property or indexer 'Person.LastName' can only be assigned in an object initializer,
//               or on 'this' or 'base' in an instance constructor or an 'init' accessor.
// pet.LastName = "Kowalski";
}
*/
public class Pet
{
    public required int Age;
    public string Name;
}
/*
// `Age` field is necessary to be initialized.
// You don't need to initialize `Name` property
var pet = new Pet() { Age = 10 };

// Compiler error:
// Error CS9035 Required member 'Pet.Age' must be set in the object initializer or attribute constructor.
// var pet = new Pet();
*/