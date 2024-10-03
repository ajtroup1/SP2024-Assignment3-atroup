# .NET MVC C# Movie and Actor Management Website Checklist

## Project Overview

- [ ] Create a .NET MVC C# website dedicated to managing movies and actors.
- [ ] Publish the website to Azure.

## Home Page

- [ ] Create a home page with an introduction to what the site is about.

## Movie Management Page

- [ ] Create a page to manage multiple movies with the following details:
  - [ ] Title
  - [ ] IMDB Hyperlink
  - [ ] Genre
  - [ ] Year of release
  - [ ] Poster or media
- [ ] Add at least 4 movies with relevant details.

### Movie Details Page

- [ ] Implement a section on the movie details page to call Reddit and Sentiment Analysis APIs:
  - [ ] Display a table containing:
    - [ ] The top 100 Reddit pages related to the selected movie using the Reddit API.
    - [ ] The analyzed sentiment of all Reddit pages (Note: Don't count a zero score).
  - [ ] Display a heading above the table with the overall sentiment analysis for the selected movie.

## Actor Management Page

- [ ] Create a page to manage multiple actors with the following details:
  - [ ] Name
  - [ ] Gender
  - [ ] Age
  - [ ] IMDB Hyperlink
  - [ ] Photo of the actor
- [ ] Add at least 8 actors with relevant details.

### Actor Details Page

- [ ] Implement a section on the actor details page to call Reddit API and VADER Sentiment Analysis packages:
  - [ ] Display a table containing:
    - [ ] The top 100 Reddit pages related to the selected actor using the Reddit API.
    - [ ] The analyzed sentiment of all Reddit pages (Note: Don't count a zero score).
  - [ ] Display a heading above the table with the overall sentiment analysis for the selected actor.

## Actor-Movie Linking Page

- [ ] Create a page that can list actors in movies and manage them (similar to the "students with pets" demo from class).

## Styling

- [ ] Apply styling to the website using:
  - [ ] A CSS theme from [jQueryUI](https://jqueryui.com/themeroller/) or [Bootswatch](https://bootswatch.com/4/).
- [ ] Ensure that the website has been properly styled (points will be deducted for little or no effort).

## Additional Requirements

- [ ] Leverage view models to get Reddit pages and other info to a details page.

### Recommended NuGet Packages

- [ ] Install the following NuGet package:
  - [ ] `VaderSharp2` (Sentiment)
  - [ ] Go to `Tools` -> `NuGet Package Manager` -> `Manage NuGet Packages for Solution` -> Browse -> Search `VaderSharp2` -> Check the box for the solution on the right -> Install

## Sample Code Checklist

- [ ] Use the provided code to search Reddit for text related to movies and actors.
- [ ] Implement the missing `CategorizeSentiment` function (use ChatGPT to assist).

### Sentiment Analysis Code Example
- [ ] Ensure the sentiment analysis doesn't count values with a compound score of zero.
- [ ] Ensure that the total number of items excludes those with a zero score.

```csharp
public static async Task<List<string>> SearchRedditAsync(string searchQuery)
{
    var returnList = new List<string>();
    var json = "";
    using (HttpClient client = new HttpClient())
    {
        // fake like you are a "real" web browser
        client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
        json = await client.GetStringAsync("https://www.reddit.com/search.json?limit=100&q=" + HttpUtility.UrlEncode(searchQuery));
    }
    var textToExamine = new List<string>();
    JsonDocument doc = JsonDocument.Parse(json);
    // Navigate to the "data" object
    JsonElement dataElement = doc.RootElement.GetProperty("data");
    // Navigate to the "children" array
    JsonElement childrenElement = dataElement.GetProperty("children");
    foreach (JsonElement child in childrenElement.EnumerateArray())
    {
        if (child.TryGetProperty("data", out JsonElement data))
        {
            if (data.TryGetProperty("selftext", out JsonElement selftext))
            {
                string selftextValue = selftext.GetString();
                if (!string.IsNullOrEmpty(selftextValue)) { returnList.Add(selftextValue); }
                else if (data.TryGetProperty("title", out JsonElement title)) // use title if text is empty
                {
                    string titleValue = title.GetString();
                    if (!string.IsNullOrEmpty(titleValue)) { returnList.Add(titleValue); }
                }
            }
        }
    }
    return returnList;
}
```

## Extra Credit (Up to 10 Points)

- [ ] Use [DataTables](https://datatables.net/) to manage all tables, especially the Reddit posts.
- [ ] Add movies to the actors page and actors to the movies page for better interlinking.

## Submission

- [ ] Turn in the link to your published Azure site.
- [ ] Turn in the GitHub repository link.

