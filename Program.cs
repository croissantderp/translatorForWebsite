using System.Text.RegularExpressions;

//getting text from a file
Console.WriteLine("Enter file to process");
string inputFilePath = Console.ReadLine() ?? "no path";

inputFilePath = inputFilePath.Replace("\"", "");

if (!File.Exists(inputFilePath))
{
    Console.WriteLine($"No file found at {inputFilePath}\nPress any key to exit");
    Console.ReadKey();
    return;
}

Regex splitter = new Regex(@"(?<=^[^""]*(?:""[^""]*""[^""]*)*)\n(?=(?:[^""]*""[^""]*"")*[^""]*$)");

string[] projectsArray = splitter.Split(File.ReadAllText(inputFilePath)).Where(a => a != "" && !Regex.IsMatch(a, @"^(header|sum)")).ToArray();

    Console.WriteLine($"Done reading input file ({projectsArray.Length} items)");
//generates HTML for each project in the list
string[] finalString = { "", "", "" };
foreach (string project in projectsArray)
{   
    string[] fields = project.Split("\t");

    string toAdd = "";

    //checks if it is the beginning of a group
    if (Regex.IsMatch(fields[0], @"^\[\[\[\["))
    {
        Console.WriteLine("beginning group");

        //adds the cover and div for the group
        toAdd = 
            $"<div class=\"group {fields[0].Replace("[[[[", "")}\">\n" +
            $"<input type=\"checkbox\" />\n" +
            $"<div class=\"cover\">\n" +
            $"<div class=\"square\"><img src=\"./assets/{fields[7]}\" /></div>\n" +
            $"<div class=\"background\"></div>\n" +
            $"<div class=\"arrow\"></div>\n" +
            $"</div>\n" +
            $"<div class=\"desc\">\n" +
            $"<p>{fields[2]}</p>\n" +
            $"</div>\n\n";
    }
    //checks if it is the end of a group
    else if (Regex.IsMatch(fields[0], @"^\]\]\]\]"))
    {
        //closes the div opened by the beginning of the group
        toAdd = "</div>\n\n";
    }
    //regular project processiing
    else
    {
        fields[2] = Regex.Replace(fields[2], "(^\"|\"$)", "").Replace("\"\"", "\"").Replace("\r\n", "");

        //generates description and processes image and link information
        string[] description = Regex.Split(fields[2], @"\[\[|\]\]").Where(a => a != "").ToArray();

        string[] links = fields[8].Replace("\r", "") != "null" ? fields[8].Replace("\r", "").Split(",").Select(
            a => "<a href=\"" + a.Split("---")[1] + "\" target=\"none\">" + a.Split("---")[0] + "</a>\n").ToArray() : new string[] { "null" };

        //determines whether odd or even indices are images
        int remainder = Regex.IsMatch(fields[2], @"^\[\[") ? 0 : 1;

        string linksText = links[0] != "null" ? "<p class=\"a_container\">\n" + string.Join(" • ", links) + "</p>" : "";

        description = description.Select(
            //figures out whether the current item is media or text
            (a, i) => (Regex.Match(a, @"\.[a-zA-Z0-9]{3}(,|$)").Success || a == "break") ?
            //generates for images or videos
            descText(a, i) :
            //generates text
            (Regex.Match(a, @"^[ ]+$").Success ? "" : "<p>" + a + "</p>\n")
            //adds links to description
            ).Append(linksText).ToArray();


        string field9 = fields[9].Replace("\r", "") == "" ? "" : $"<p class=\"blurb\">{fields[9].Replace("\r", "")}</p>";

        string year = fields[10].Replace("\r", "") == "" ? "" : $"<p class=\"year\">{Regex.Replace(fields[10].Replace("\r", ""), @"^20", "\'")}</p>\n";


        toAdd = $"<div class=\"item\" id=\"{fields[0].Replace("$", "").Replace(" ", "-")}\">\n" +
                $"<input type=\"checkbox\" />\n" +
                $"<div class=\"square\"><img src=\"./assets/{fields[7]}\" /></div>\n" +
                $"<div class=\"background\"></div>\n<div class=\"arrow\"></div>\n" +
                $"{year}\n" +
                (!fields[0].Contains("$$") ? $"<h1>{fields[0]}</h1>\n" : "") +
                $"<div class=\"details\">\n" +
                $"<h2>{fields[0].Replace("$", "")}</h2>\n" +
                $"{field9}" +
                $"{string.Join("", description).Replace("<br/>", "</p><p>")}\n" +
                $"</div>\n" +
                $"</div>\n\n";
    }

    //adds the project to 
    //3d printing
    if (fields[4] == "1")
    {
        finalString[0] += toAdd;
    }
    //art
    if (fields[5] == "1")
    {
        finalString[1] += toAdd;
    }
    //coding
    if (fields[6] == "1")
    {
        finalString[2] += toAdd;
    }

    Console.WriteLine("Done processing " + fields[0]);
}

//writes output to 3 files for each category
for (int i = 0; i < finalString.Length; i++)
{
    if (finalString[i] == "")
    {
        continue;
    }

    string add = "";
    switch (i)
    {
        case 0:
            add = "3dPrint";
            break;
        case 1:
            add = "art";
            break;
        case 2:
            add = "coding";
            break;
    }

    File.WriteAllText(inputFilePath + "-output-" + add + ".txt", finalString[i]);
}

Console.WriteLine("Done writing output to file");

string descText(string a, int i)
{
    string[] temp = Regex.Split(a, @"(?<!\\),");

    if (a.Split(",")[0].Split(".").Last() == "mp4")
    {
        return $"<video controls muted>\n<source src=\"./assets/{temp[0].Replace("\\", "")}\" type=\"video/mp4\" />\nSorry, your browser doesn't support embedded videos.\n</video>\n";
    }
    else if (a == "break")
    {
        return $"<div class=\"break\"></div>";
    }

    return  "<img src=\"./assets/" + temp[0].Replace("\\", "") + "\" title=\"" + temp[1].Replace("\\", "") + "\" alt=\"" + temp[1].Replace("\\", "") + "\"/>\n";
}