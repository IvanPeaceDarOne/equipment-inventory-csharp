## A Short Description

This is my first serious project on my path to becoming a junior developer using the .NET platform and the C# programming language.

It is a console equipment registry built with collections.

The program stores equipment in memory and processes commands entered one line at a time. The data is not saved to a file, so the registry is cleared after the program ends.

Failed commands do not change the state of the registry.

All logic is built with methods, without using OOP. This was done intentionally to improve my current knowledge before moving on to a deeper study of OOP.

## Running in Visual Studio

1. Open the `EquipmentInventory.slnx` file in Visual Studio.
2. Press `Ctrl + F5` to run the program without debugging.

## Running Through the Terminal 

1. Open a terminal in the folder that contains the `EquipmentInventory.csproj` file.
2. Run:
```powershell
dotnet run
```

## Table of All Commands and Formats

Commands are entered without spaces between fields and use `;` as a separator.

Examples:

| Command | Format | Purpose |
|---|---|---|
| Add            | `add;CODE;NAME;QTY;PRICE;DATE` | Adds a new equipment entry. Duplicate codes are rejected. 
| Receive        | `receive;CODE;QTY`             | Increases the quantity in stock. 
| Issue          | `issue;CODE;QTY`               | Decreases the quantity in stock.
| Find           | `find;CODE`                    | Shows one entry by its code. 
| List           | `list`                         | Shows entries in the order they were successfully added. 
| Due date check | `due;DATE`                     | Shows entries where the due date is less than or equal to the given date. 
| Total value    | `total`                        | Shows the sum of `QTY x PRICE` for all entries. 
| Exit           | `quit`                         | Ends the program. 
 
## Field Formats and Rules

| Field | Rule |
|---|---|
| `CODE`                       | From 1 to 12 ASCII letters or digits. For example: `A1`, `MONITOR2`. 
| `NAME`                       | From 1 to 40 characters after `Trim()`. 
| `QTY` in `add`               | An integer from `0` to `100000`. 
| `QTY` in `receive` / `issue` | An integer from `1` to `100000`. 
| `PRICE`                      | A `decimal` number from `0.01` to `1000000`, with a dot as the decimal separator and no more than two digits after the dot. 
| `DATE`                       | Strictly `yyyy-MM-dd`, for example `2026-12-31`. 

## Possible Errors and Their Reasons

| Error | Example | Reason |
|---|---|---|
| `FORMAT`    | `add;Lamp;3;12.50;2026-10-20`                                                                    | The command has the wrong number of fields. |
| `VALUE`     | `add;a-1;Lamp;3;12.50;2026-10-20`                                                                | The command has enough fields, but one or more values do not meet the program rules. |
| `DUPLICATE` | First input: `add;a1;Lamp;3;12.50;2026-10-20` <br>Second input: `add;A1;Lamp;3;12.50;2026-10-20` | An attempt to add equipment with a code that already exists in the collection. |
| `MISSING`   | First input: `add;a1;Lamp;3;12.50;2026-10-20` <br>Second input: `find;b2`                        | The requested equipment could not be found. |
| `STOCK`     | First input: `add;a1;Lamp;3;12.50;2026-10-20` <br>Second input: `issue;a1;5`                     | After the operation, the equipment quantity would go outside the allowed limits. |
| `UNKNOWN`   | `search;a1;Lamp;3;12.50;2026-10-20`                                                              | An unknown command was entered. |

## Example

Input:

```text
add; a1 ; Lamp ;3;12.50;2026-10-20
receive;A1;2
find;a1
total
quit
```

Output:

```text
OK A1 3
OK A1 5
A1;Lamp;5;12.50;2026-10-20
TOTAL 62.50
```

## Chosen Types and Data Structures

### `Dictionary<string, (string Name, int Quantity, decimal Price, DateOnly Due)>`

This collection was chosen for convenient equipment lookup by key in the `find`, `receive`, and `issue` commands. With the `StringComparer.OrdinalIgnoreCase` modifier, codes become case-insensitive: "a1" is equal to "A1".

Because of the "no OOP" requirement, the collection was also chosen as a convenient way to store names, quantities, prices, and dates without creating a separate class.

### `List<string> codeList`

This list was chosen to store all added equipment codes in insertion order. Because of this, the `list` and `due` commands print entries in the order they were added to the collection.

### `decimal`

`decimal` was chosen for accurate price calculations because it is more suitable for money calculations than `double`. This reduces the risk of rounding errors.

### `DateOnly`

`DateOnly` is used to work with due dates. The program does not require a time or time zone, and `TryParseExact` guarantees the `yyyy-MM-dd` date format and safely rejects invalid dates.
