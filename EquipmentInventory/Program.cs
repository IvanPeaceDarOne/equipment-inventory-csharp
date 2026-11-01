using System.Globalization;

namespace EquipmentInventory
{
    internal class Program
    {
        //Method that handles all possible errors
        static private bool printError(Dictionary<string, (string Name, int Quantity, decimal Price, DateOnly Due)> Equipment, List<string> splitResult, int needCount, bool isNeedCheckCode = true, List<string>? codeList = null)
        {
            string CODE = "";
            string COMMAND = splitResult[0].Trim().ToLower();

            if (splitResult.Count != needCount)
            {
                Console.WriteLine("ERROR FORMAT");
                return true;
            }

            if (isNeedCheckCode &&
                !IsValidCode(splitResult[1].Trim().ToUpperInvariant())) //Проверка на ASCII символы в коде
            {
                Console.WriteLine("ERROR VALUE");
                return true; 
            }

            //This switch uses the required number of fields to determine which method called printError
            //and selects the appropriate validation logic for that method
            switch (needCount)
            {
                //Case that validates the split result for the add method
                case 6:
                    CODE = splitResult[1].Trim().ToUpperInvariant();
                         
                    string NAME = splitResult[2].Trim();
                    if (!(NAME.Length >= 1 && NAME.Length <= 40))
                    {
                        Console.WriteLine("ERROR VALUE");
                        return true;
                    }

                    if (!int.TryParse(splitResult[3].Trim(), out int QUANTITY) || QUANTITY < 0 || QUANTITY > 100000)
                    {
                        Console.WriteLine("ERROR VALUE");
                        return true;
                    }

                    //Store the price as text so we can check that it has no more than two digits after the decimal point
                    string priceText = splitResult[4].Trim();

                    //A few helper bool variables used to validate the price

                    //Parse the price, allowing a decimal point through NumberStyles
                    bool isPriceParsed = decimal.TryParse(priceText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal PRICE);
   
                    bool isPriceInRange = PRICE >= 0.01m && PRICE <= 1000000m;

                    int dotIndex = priceText.IndexOf('.');

                    int digitsAfterDot = priceText.Length - dotIndex - 1;

                    /*Check that the price has a valid number of digits after the decimal point:
                    12     - accept
                    12.    - reject
                    12.5   - accept
                    12.45  - accept
                    12.345 - reject
                    */
                    bool hasValidDecimalPlaces = dotIndex == -1 || (digitsAfterDot >= 1 && digitsAfterDot <= 2);

                    //Check all previously created bool variables
                    if (!isPriceParsed || !hasValidDecimalPlaces || !isPriceInRange)
                    {
                        Console.WriteLine("ERROR VALUE");
                        return true;
                    }

                    if (!DateOnly.TryParseExact(splitResult[5].Trim(), "yyyy-MM-dd", out DateOnly DATE))
                    {
                        Console.WriteLine("ERROR VALUE");
                        return true;
                    }

                    if (Equipment.ContainsKey(CODE))
                    {
                        Console.WriteLine("ERROR DUPLICATE");
                        return true;
                    }
                    break;

                //Block that validates errors for methods where the split result has 3 fields (receive, issue)
                case 3:
                    CODE = splitResult[1].Trim().ToUpperInvariant(); 

                    if (!Equipment.ContainsKey(CODE))
                    {
                        Console.WriteLine("ERROR MISSING");
                        return true;
                    }

                    if (!int.TryParse(splitResult[2].Trim(), out int Quantity) || Quantity < 1 || Quantity > 100000)
                    {
                        Console.WriteLine("ERROR VALUE");
                        return true;
                    }

                    switch (COMMAND)
                    {//Check that the quantity does not exceed the allowed limits depending on whether it is being added or removed
                        case "receive":
                            
                            if (Equipment[CODE].Quantity + Quantity > 100000)
                            {
                                Console.WriteLine("ERROR VALUE");
                                return true;
                            }
                            break;
                        case "issue":
                            
                            if (Quantity > Equipment[CODE].Quantity)
                            {
                                Console.WriteLine("ERROR STOCK");
                                return true;
                            }
                            break;
                    }

                    break;

                //Block that validates errors for methods where the split result has 2 fields (find, due)
                case 2:

                    switch (COMMAND)
                    {
                        case "find":

                            CODE = splitResult[1].Trim().ToUpperInvariant();

                            if (!Equipment.ContainsKey(CODE))
                            {
                                Console.WriteLine("ERROR MISSING");
                                return true;
                            }
                            break;
                        case "due":

                            if (!DateOnly.TryParseExact(splitResult[1].Trim(), "yyyy-MM-dd", out DateOnly date))
                            {
                                Console.WriteLine("ERROR VALUE");
                                return true;
                            }
                            break;
                    }

                    break;

                //Block that validates errors for methods where the split result has 1 field (list, total)
                case 1:
                    switch (COMMAND)
                    {
                        case "list":

                            if (Equipment.Count == 0 || codeList?.Count == 0)
                            {
                                Console.WriteLine("EMPTY");
                                return true;
                            }
                            break;
                        //The total method does not need any extra validation, but it is kept here to match the switch structure
                        case "total": break;

                    }
                    
                    break;
            }
            return false;
        }

        //Method that checks whether a collection code contains only ASCII letters and digits
        static bool IsValidCode(string Code)
        {

            if (Code.Length <= 0 || Code.Length > 12) return false;

            //Loop through each character in the code and check whether it is an ASCII letter or digit
            foreach (char symbol in Code)
            {
                bool validLetter = symbol >= 'A' && symbol <= 'Z';
                bool validDigit = symbol >= '0' && symbol <= '9';

                if (!validLetter && !validDigit) return false;
              
            }
            return true;
        }

        //Method that adds equipment to the collection
        static Dictionary<string, (string Name, int Quantity, decimal Price, DateOnly Due)> Add(List<string> splitResult, Dictionary<string, (string Name, int Quantity, decimal Price, DateOnly Due)> Equipment, List<string> codeList)
        {
            const int needCount = 6;

            if (printError(Equipment,splitResult,needCount)) return Equipment;


            //Parse all fields from the string list before adding the equipment to the collection


            string CODE = splitResult[1].Trim().ToUpperInvariant();

            string NAME = splitResult[2].Trim();

            int QUANTITY = int.Parse(splitResult[3].Trim());                                                
                                                                                                       
            decimal PRICE = decimal.Parse(splitResult[4].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

            DateOnly DATE = DateOnly.ParseExact(splitResult[5].Trim(), "yyyy-MM-dd");


            //Add the equipment to the collection, then save its code in codeList
            Equipment.Add(CODE, (NAME, QUANTITY, PRICE, DATE));
            codeList.Add(CODE);


            Console.WriteLine($"OK {CODE} {QUANTITY}");

            return Equipment;
          
        }

        //Method that adds the given quantity to the current equipment quantity
        static void Receive(Dictionary<string, (string Name, int Quantity, decimal Price, DateOnly Due)> equipment, List<string> splitResult)
        {
            const int needCount = 3;

            if (printError(equipment, splitResult, needCount)) return;      

            string CODE = splitResult[1].Trim().ToUpperInvariant();

            int receiveQuantity = int.Parse(splitResult[2].Trim());

            //Create a temporary copy because tuple fields cannot be updated directly in the dictionary
            //After changing the quantity, write the whole tuple back to the collection
            var temp = equipment[CODE];         

            int newQuantity = temp.Quantity + receiveQuantity;

            //Replace the entry by its code while keeping all fields except the quantity unchanged
            equipment[CODE] = (temp.Name, newQuantity, temp.Price, temp.Due);

            Console.WriteLine($"OK {CODE} {newQuantity}");
        }

        //Method that subtracts the given quantity from the current equipment quantity
        static void Issue(Dictionary<string, (string Name, int Quantity, decimal Price, DateOnly Due)> equipment, List<string> splitResult)
        {
            const int needCount = 3;

            if (printError(equipment, splitResult, needCount)) return;

            string CODE = splitResult[1].Trim().ToUpperInvariant();

            int issueQuantity = int.Parse(splitResult[2].Trim());

            //Create a temporary copy because tuple fields cannot be updated directly in the dictionary
            //After changing the quantity, write the whole tuple back to the collection
            var temp = equipment[CODE];

            int newQuantity = temp.Quantity - issueQuantity;

            //Replace the entry by its code while keeping all fields except the quantity unchanged
            equipment[CODE] = (temp.Name, newQuantity, temp.Price, temp.Due);

            Console.WriteLine($"OK {CODE} {newQuantity}");
        }

        //Method that finds an entry by its code and prints it to the console if it exists
        static void Find(Dictionary<string, (string Name, int Quantity, decimal Price, DateOnly Due)> equipment, List<string> splitResult)
        {
            const int needCount = 2;

            if (printError(equipment, splitResult, needCount)) return;
           
            string CODE = splitResult[1].Trim().ToUpperInvariant();

            Console.WriteLine($"{CODE};{equipment[CODE].Name};{equipment[CODE].Quantity};{equipment[CODE].Price.ToString("F2", CultureInfo.InvariantCulture)};{equipment[CODE].Due.ToString("yyyy-MM-dd")}");

        }

        //Method that prints equipment from the collection in the order it was added
        static void List(Dictionary<string, (string Name, int Quantity, decimal Price, DateOnly Due)> equipment, List<string> codeList, List<string> splitResult)
        {
            const int needCount = 1;

            if (printError(equipment, splitResult, needCount, false, codeList)) return;

            //Loop that prints each collection entry in the order it was added
            foreach (var code in codeList)
            {
                Console.WriteLine($"{code};{equipment[code].Name};{equipment[code].Quantity};{equipment[code].Price.ToString("F2", CultureInfo.InvariantCulture)};{equipment[code].Due.ToString("yyyy-MM-dd")}");
            }
        }

        //Method that finds equipment with a due date less than or equal to the given date
        static void Due(Dictionary<string, (string Name, int Quantity, decimal Price, DateOnly Due)> equipment, List<string> codeList, List<string> splitResult)
        {
            const int needCount = 2;

            if (printError(equipment, splitResult, needCount, false)) return;

            DateOnly DATE = DateOnly.ParseExact(splitResult[1].Trim(), "yyyy-MM-dd");

            bool dateFound = false;

            //Loop through all added collection entries and find ones with a due date less than or equal to the given date
            foreach (var code in codeList)
            {
                if (equipment[code].Due <= DATE)
                {
                    Console.WriteLine($"{code};{equipment[code].Name};{equipment[code].Quantity};{equipment[code].Price.ToString("F2", CultureInfo.InvariantCulture)};{equipment[code].Due.ToString("yyyy-MM-dd")}");
                    dateFound = true;
                }
            }

            if (!dateFound) Console.WriteLine("EMPTY");
 
        }

        //Method that prints the total value of all equipment in the collection
        static void Total(Dictionary<string, (string Name, int Quantity, decimal Price, DateOnly Due)> equipment, List<string> codeList, List<string> splitResult)
        {
            const int needCount = 1;

            if (printError(equipment, splitResult, needCount, false)) return;

            decimal total = 0;

            //Loop through the collection in insertion order, multiply each quantity by its price, and add the result to total
            foreach (var code in codeList)
            {
                total += (equipment[code].Quantity * equipment[code].Price);
            }
 
            Console.WriteLine($"TOTAL {total.ToString("F2",CultureInfo.InvariantCulture)}");

        }



        static void Main(string[] args)
        {

            //Working variables
            string? readResult = "";
            var codeList = new List<string>();
            var equipment = new Dictionary<string, (string Name, int Quantity, decimal Price, DateOnly Due)>(StringComparer.OrdinalIgnoreCase);


            //Loop that processes console input
            do
            {
                readResult = Console.ReadLine();

                if (readResult == null) break;
   
                if (readResult.Trim().ToLower() == "quit") break;

                //Check for empty console input and skip it without printing anything
                if (readResult.Trim() == "") continue;

                var splitResult = new List<string>(readResult.Split(';'));

                //Switch that chooses which method to use based on the command
                switch (splitResult[0].Trim().ToLower())
                {
                    case "add":
                        equipment = Add(splitResult,equipment,codeList);                 
                        break;
                    case "receive":
                        Receive(equipment, splitResult);
                        break;
                    case "issue":
                        Issue(equipment, splitResult);
                        break;
                    case "find":
                        Find(equipment, splitResult);
                        break;               
                    case "list":
                        List(equipment, codeList, splitResult);
                        break;
                    case "due":
                        Due(equipment, codeList, splitResult);
                        break;
                    case "total":
                        Total(equipment, codeList,splitResult);
                        break;
                    default:
                        Console.WriteLine("ERROR UNKNOWN");
                        break;
                 
                }

            } while (true);

            


        }
    }
}
