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

string[] projectsArray = File.ReadAllText(inputFilePath).Split("\n").Where(a => a != "" && !Regex.IsMatch(a, @"^(header|sum)")).ToArray();

Console.WriteLine("Done reading input file");

//generates HTML for each project in the list
string[] finalString = { "", "", "" };
foreach (string project in projectsArray)
{
    string[] fields = project.Split("\t");

    //generates description and processes image and link information
    string[] description = Regex.Split(fields[2], @"\[\[|\]\]").Where(a => a != "").ToArray();

    string[] links = fields[8].Replace("\r", "").Split(",").Select(
        a => "<a href=\"" + a.Split("---")[1] + "\" target=\"none\">" + a.Split("---")[0] + "</a>\n").ToArray();

    //determines whether odd or even indices are images
    int remainder = Regex.IsMatch(fields[2], @"^\[\[") ? 0 : 1;

    description = description.Select(
        //figures out whether the current item is media or text
        (a, i) => i % 2 == remainder ? 
        //generates for images or videos
        (a.Split(",")[0].Split(".").Last() == "mp4" ? 
            $"<video controls>\n<source src=\"./assets/{Regex.Split(a, @"(?<!\\),")[0].Replace("\\", "")}\" type=\"video/mp4\" />\nSorry, your browser doesn't support embedded videos.\n</video>\n" : 
            "<img src=\"./assets/" + Regex.Split(a, @"(?<!\\),")[0].Replace("\\", "") + "\" title=\"" + Regex.Split(a, @"(?<!\\),")[1].Replace("\\", "") + "\" alt=\"" + Regex.Split(a, @"(?<!\\),")[1].Replace("\\", "") + "\"/>\n") : 
        //generates text
        "<p>" + a + "</p>\n"
        //adds links to description
        ).Append("<p class=\"a_container\">\n" + string.Join(" • ", links) + "</p>").ToArray();

    //adds the project to 
    //3d printing
    if (fields[4] == "1")
    {
        finalString[0] += 
            $"<div class=\"item\" id=\"{fields[0].Replace(" ", "-")}\">\n" +
            $"<input type=\"checkbox\" />\n" +
            $"<div class=\"square\"><img src=\"./assets/{fields[7]}\" /></div>\n" +
            $"<div class=\"background\"></div>\n<div class=\"arrow\"></div>\n" +
            (!fields[0].Contains("$$") ? $"<h1>{fields[0].Replace("$", "")}</h1>\n" : "") +
            $"<div class=\"details\">\n" +
            $"<h2>{fields[0].Replace("$", "")}</h2>\n" +
            $"{string.Join("", description)}\n" +
            $"</div>\n" +
            $"</div>\n\n";
    }
    //art
    if (fields[5] == "1")
    {
        finalString[1] +=
            $"<div class=\"item\" id=\"{fields[0].Replace(" ", "-")}\">\n" +
            $"<input type=\"checkbox\" />\n" +
            $"<div class=\"square\"><img src=\"./assets/{fields[7]}\" /></div>\n" +
            $"<div class=\"background\"></div>\n<div class=\"arrow\"></div>\n" +
            (!fields[0].Contains("$$") ? $"<h1>{fields[0].Replace("$", "")}</h1>\n" : "") +
            $"<div class=\"details\">\n" +
            $"<h2>{fields[0].Replace("$", "")}</h2>\n" +
            $"{string.Join("", description)}\n" +
            $"</div>\n" +
            $"</div>\n\n";
    }
    //coding
    if (fields[6] == "1")
    {
        finalString[2] +=
            $"<div class=\"item\" id=\"{fields[0].Replace(" ", "-")}\">\n" +
            $"<input type=\"checkbox\" />\n" +
            $"<div class=\"square\"><img src=\"./assets/{fields[7]}\" /></div>\n" +
            $"<div class=\"background\"></div>\n<div class=\"arrow\"></div>\n" +
            (!fields[0].Contains("$$") ? $"<h1>{fields[0].Replace("$", "")}</h1>\n" : "") +
            $"<div class=\"details\">\n" +
            $"<h2>{fields[0].Replace("$", "")}</h2>\n" +
            $"{string.Join("", description)}\n" +
            $"</div>\n" +
            $"</div>\n\n";
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