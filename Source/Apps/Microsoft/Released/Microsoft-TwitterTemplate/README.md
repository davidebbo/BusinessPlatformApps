Brand & Campaign Management Solution Template Documentation
===========================================================

Introduction
============

The Twitter template spins up a complete brand/campaign solution. It stands up an end-to-end solution that pulls data from Twitter, enriches the data using machine learning and stores it in Azure SQL. Users can then use pre-built Power BI reports that leverage Microsoft research technology to start analyzing their Twitter data and augmenting it with additional data sources.

The template is aimed at anyone who is interesting in analyzing Twitter data. It supports multiple personas ranging from a marketing manager monitoring the success of their campaign to a ministry official interested in tracking the public’s reactions on social media.

The template lets you do things like:

-   Understand what aspects of your product/campaign/event are well received and immediately spot the biggest concerns and pain points

-   Track what topics are trending and how that changes across time

-   Identify your biggest influencers fans and critics

The following document provides a walkthrough of the architecture, a deep dive into every component, comments on customizability as well as information on additional topics like pricing. For any questions not covered in this document, please contact the team at <PBISolnTemplates@microsoft.com>

Architecture
------------

![Image](Resources/media/image1.png)

The flow of the Twitter solution template is as follows:

-   Logic Apps pull data from Twitter

-   Logic App passes tweet into an Azure Function

-   Azure Function enriches tweet and writes it to Azure SQL

-   Power BI imports data into it from Azure SQL and renders pre-defined reports

System Requirements
===================

Setting up the template requires the following:

-   Access to an Azure subscription

-   Power BI Desktop (latest version)

-   Power BI Pro (to share the template with others)

-   Twitter Account

How to Install
==============

Before diving into the components of the solution, we will go through how to set things up. To get started with the solution, navigate to the [Twitter template page](https://powerbi.microsoft.com/en-us/solution-templates/brand-management-twitter) and click **Install Now**.

**Getting Started:** Starting page introducing the template and explaining the architecture.

![Image](Resources/media/image2.png)

**Azure:** Use OAuth to sign into your Azure account. You will notice you have a choice between signing into an organizational account and a Microsoft (work/school account).

If you select a Microsoft account, you will need to provide the application with a domain directory. You can find your domain by logging into your Azure account and choosing from those listed when you click your e-mail in the top right hand corner:

![Image](Resources/media/image3.png)
If you belong to a single domain, simply hover over your e-mail address in the same place:

![Image](Resources/media/image4.png)

In this case, the domain is: richtkhotmail.362.onmicrosoft.com.

![Image](Resources/media/image5.png)
Logging into Azure gives the application access to your Azure subscription and permits spinning up Azure services on your behalf. As a user navigates away from this page a new resource group gets spun up on their Azure subscription (the name is random but always prefixed by ‘SolutionTemplate-‘). All newly created resources go into this container.

**Target:** Connect to an existing SQL Server or provide details which the application will use to spin up an Azure SQL on your behalf. Only Azure SQL is supported for this template. If a user chooses to spin up a new Azure SQL, this will get deployed in their Azure subscription inside the newly created resource group.

![Image](Resources/media/image6.png)

**Twitter:** Use OAuth to sign into a Twitter account. The application will sign into this Twitter account inside the Logic App and use it to pull tweets (this will not have any impact on your Twitter account). We will never post tweets on behalf of a user.

![Image](Resources/media/image7.png)

**Search Terms:** Input the search terms you are interested in tracking. Tweets that match your search terms will be pulled from Twitter via the Logic App. Logic Apps supports any queries that the Twitter Search API supports. Guidelines are available [here](https://dev.twitter.com/rest/public/search). If you would like to learn how you can change your search terms once the solution is deployed, please look at the ‘Customizations’ section.

![Image](Resources/media/image8.png)

**Twitter Handles:** The template can enrich the data that comes in with the tweet direction. In order to do that it needs to know which specific Twitter handles you are interested in tracking. Please input these into the Twitter handle box as demonstrated in the example. If you would like to learn how you can change your Twitter Handles after the solution is deployed, please look at the ‘Customizing solution’ section.

![Image](Resources/media/image9.png)

**Summary:** Summary page outlining all the choices the user made.

![Image](Resources/media/image10.png)

**Deploy:** When you navigate to the deployment page the setup process gets kicked off. SQL scripts run to create the necessary tables and views. An Azure Function then gets spun up on your Azure subscription. This step could take even 5 minutes as required Python packages need to be uploaded. Finally, a Logic App is created that has a connection to your Azure Function.

**It is important that you do not navigate away from this page while deployment takes place.** Once everything gets deployed a download link will appear for a Power BI file which consists of the pre-defined reports.

![Image](Resources/media/image11.png)

**Power BI File:** Once you download the Power BI desktop file you will need to connect it to your data. Open the pbix and follow the instructions on the front page (**it is important you do this before publishing the file to the Power BI Service. If this is not done the solution will not work inside the Service).**

![Image](Resources/media/image12.png)

Architecture Deep Dive
======================

The following section will break down how the template works by going through all the components of the solution.

![Image](Resources/media/image1.png)

Azure Resources:
----------------

You can access all of the resources that have been spun up by logging into the Azure portal. Everything should be under one resource group (unless a user was using an existing SQL server. In this case the SQL Server will appear in whatever resource group it already existed in).

![Image](Resources/media/image13.png)Here is an example of what gets spun up for a user. We will go through each of these items one by one:

![Image](Resources/media/image14.png)

### Logic App:

Logic Apps are an Azure service for building integrated solutions. You can easily build business logic flows that consist of various actions and triggers. This is the workflow we spin up using our template:

![Image](Resources/media/image15.png)

The Logic App consists of two steps: a **trigger** (when a new tweet appears) and an **action** (Azure Function running python script).

In this particular example, the Logic App tracks all tweets that contain the phrase ‘Game of Thrones’. When a new tweet is found that contains this phrase, the JSON body of the tweet gets passed into an Azure Function. You can customize the search queries in your trigger whenever you want. Updating the search queries will update which tweets get collected.

You can also customize the frequency at which tweets are brought in. In this example, Twitter gets scanned every 3 minutes and tweets that were created in the past 3 minutes and that match the search terms get brought in.

Whatever tweets are found in the 3-minute interval, are batched up and sent sequentially through the Azure Function. We will go into more detail about what gets done inside the function next.

![Image](Resources/media/image16.png)

### Azure Function: 

A JSON payload of the tweet gets sent into the newly created Azure Function which consists of a Python script. When the function was spun up the run file was updated to the specific script below. FTP was also used to upload all the required python packages for the script to run (you cannot install Python packages/modules from scratch inside the Azure function environment without using FTP).

The first part of the script imports all the required packages for the solution:

![Image](Resources/media/image17.png)

The sentence tokenizer and lexicon dictionaries also get imported:

![Image](Resources/media/image18.png)

The following line is important - this is where the Azure Function reads in the output from the Logic App (JSON tweet). It saves it as ‘input’:

![Image](Resources/media/image19.png)

The following deserializes the tweet and saves it into a Python dictionary:

![Image](Resources/media/image20.png)

The application then creates a connection to your SQL Server (the SQL defined during the setup). The SQL details are never shown in clear text –parameters are used that get passed into the Azure function:

![Image](Resources/media/image21.png)

The next step is to read in all the twitter handles the user wants to track (this was defined on the Twitter Handles page). The twitter handles are written into a SQL ‘Configuration table’ and are then referenced here. Both twitter handles and twitter handle IDs are saved (IDs are programmatically worked out and also stored in SQL). The handles and IDs are saved into a Python dictionary.

![Image](Resources/media/image22.png)

Only English is currently supported so the first step is to check the language. Assuming the tweet is in English the sentiment is worked out. The tweet gets split into individual words, each word gets assigned a sentiment and the sentiment score gets averaged out across all the words. The score gets discretized and a categorical variable is defined which indicates whether the tweet is positive, negative or neutral.

![Image](Resources/media/image23.png)

![Image](Resources/media/image24.png)

The next step is to work out the ‘direction’ of the tweet. The script looks at the twitter handles brought in between steps 44-55 (previous page) and checks whether a match is found in the JSON body of the tweet. Since there could be a discrepancy between the tweets brought in and the twitter handles followed, it isn’t a given that a direction will be found. For this reason, the account is initialized to ‘unknown’ (line 83). The account gets overwritten and the tweet direction populated if a match is found.

![Image](Resources/media/image25.png)

![Image](Resources/media/image26.png)

The first check we do is if the tweet is a retweet or not – if it is, it goes into the block above. We then run all the specific checks to figure out where the account ID/account name is present (e.g. if the account ID we are tracking is present in the ‘tweetinreplytouserid’ property inside the JSON body of the tweet, we know the tweet is an ‘InboundReplyRT’.

Before continuing with the script, here are details about what each tweet ‘direction’ means (the example we go through is of someone following the Microsoft twitter handle):

inbound: I am tweeting ‘@Microsoft’/to:Microsoft

outbound: Tweet by ‘Microsoft’

inboundRT: I am retweeting someone else’s ‘@Microsoft’/to:Microsoft tweet

outboundRT: ‘Microsoft’ is retweeting my tweet

inboundReply: I am replying to a tweet from Microsoft

outboundReply: ‘Microsoft’ is replying to a tweet I tweeted ‘@Microsoft’/to:Microsoft

inboundReplyRT: I am retweeting a tweet that was someone tweeted @Microsoft/to:‘Microsoft’

RTofoutbound: I am retweeting a tweet that was originally posted by ‘Microsoft’

Hashtag: I am using \#Microsoft in the tweet

HashtagRT: I am retweeting a tweet that uses \#Microsoft

Text: I am tweeting with Microsoft mentioned in the tweet text (not @Microsoft or \#Microsoft)

RTText: I am retweeting a tweet that mentions Microsoft in the tweet text (not @Microsoft or \#Microsoft)

Lines 109-118 check if the tweet already exists as an entry in our tweets\_normalized table. This table stores the raw tweets along with attributes like the original author and sentiment. Since both tweets and retweets are brought in, it is possible that the tweets have been written into this table before (we don’t want to duplicate these entries – things like tweet text, **original** author and sentiment will not vary from retweet to retweet – we only want one single entry for the tweet).

The script therefore checks if the tweet exists, if it doesn’t the tweet gets written it into SQL, otherwise it gets skipped.

![Image](Resources/media/image27.png)

The next block of code is very similar to the previous in that it figures out the direction of a tweet but for non-retweets:

![Image](Resources/media/image28.png)

A similar check is done to see if the tweet already exists (due to RACE conditions it is theoretically possible to process a retweet before the original tweet, or process the same tweet twice). Assuming the tweet doesn’t exist, it gets saved into SQL.

![Image](Resources/media/image29.png)

The script then pulls out additional information about the tweet. This includes timestamps of the tweet, user details like the author’s profile image URL, their username, how many followers they have, how many things they favorited, how many friends they have and how any tweets they have tweeted.

![Image](Resources/media/image30.png)

If there are any pictures associated with the tweet the script pulls out the first image URL it finds.

![Image](Resources/media/image31.png)

We store all of this additional user, image, and timestamp information in a second table called tweets\_processed. This is joined onto the original tweet table by the tweet master id.

![Image](Resources/media/image32.png)

Hashtags and mentions from the tweet get split out and saved into separate tables. This allows you to slice the Power BI report by the hashtags and mentions that are found.

![Image](Resources/media/image33.png)

The hashtags and mentions are split out again and written into two new tables (authorhashtag\_graph and authormention\_graph). These tables save additional information like the author associated with the hashtag/mention as well as hex colors for authors and hashtags. This structures the data for the creation of network graphs which you see inside the reports.

![Image](Resources/media/image34.png)

### ![Image](Resources/media/image35.png)Storage Account

Azure Functions have a dependency on an Azure storage account and so one gets spun up as part of the automation. The storage account can be used for things like storing additional logging.

![Image](Resources/media/image36.png)

### Twitter API Connection

The Twitter API Connection is used to connect Logic Apps to your Twitter account. It contains the information you inputted to on the Twitter page in the set up process. This is what allows the application to search Twitter for your search terms via the Logic App.

![Image](Resources/media/image37.png)

### App Service Plan

Every Azure Function that gets created comes with a special hosting plan. For the Twitter template a dynamic hosting plan gets created. This means functions will be run on demand and billed per execution with no standing resource commitment. A user therefore only pays for what he or she consumes.

![Image](Resources/media/image38.png)

Azure SQL Server/Database

The final Azure resource used is Azure SQL. Azure SQL is a bit special in that you have a choice whether you want to use an existing SQL or if you would like to get a new one spun up. If you opt in for your own, you will not see this resource in the Resource Group. Alternatively, the application spins up a Standard (S0) SQL Server and database (more on pricing in the Pricing section).

The next section goes into more depth on the SQL tables and views created as part of the solution.

Model Schema
------------

Here is an overview of the tables found in the model:

| **Table Name**       | **Description **                                                                                                                                                                                                                                              |
|----------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Author Hashtag Graph | Stores hashtag and author information optimized for creating a network graph                                                                                                                                                                                  |
| Author Mention Graph | Stores mentions and author information optimized for creating a network graph                                                                                                                                                                                 |
| Configuration        | Configuration table storing parameters for the template to function correctly                                                                                                                                                                                 |
| Hashtag Slicer       | Stores every hashtag found with its corresponding tweet ID. This can be used to filter tweets to those that only contain the hashtag of interest                                                                                                              |
| Mention Slicer       | Stores every mention found with its corresponding tweet ID. This can be used to filter tweets to those that only contain the mentions of interest                                                                                                             |
| Original Tweets      | Stores original tweets with their original authors and sentiment (every tweet is unique)                                                                                                                                                                      |
| Tweets               | Stores information about every processed tweet such as the tweet direction, author and author information. It does not store the tweet itself, instead referencing the original tweet table. This table stores retweet as well as original tweet information. |
| Minimum Tweets       | A disconnected table used to filter out the minimum number of tweets to be displayed on a page. For example, if we select a minimum of 5 tweets, a chart that groups tweets by author will only display authors who have tweeted at least 5 tweets.           |

Below is a breakdown of the columns found in every table (*DAX measures and Calculated Columns in italic)*:

| **Author Hashtag Graph** |                                          |
|--------------------------|------------------------------------------|
| **Column Name**          | **Description**                          |
| Tweet ID                 | Tweet Id (includes retweeted tweets)     |
| Author                   | Author name                              |
| Author Color             | Hex color (used for network graph nodes) |
| Hashtag                  | Hashtag used in tweet (\#)               |
| Hashtag Color            | Hex color (used for network graph nodes) |

| **Author Mention Graph** |                                          |
|--------------------------|------------------------------------------|
| **Column Name**          | **Description**                          |
| Tweet ID                 | Tweet Id (includes retweeted tweets)     |
| Author                   | Author name                              |
| Author Color             | Hex color (used for network graph nodes) |
| Mention                  | Mention used in tweet (@)                |
| Mention Color            | Hex color (used for network graph nodes) |

| **Hashtag Slicer** |                                      |
|--------------------|--------------------------------------|
| **Column Name**    | **Description**                      |
| Tweet Id           | Tweet Id (includes retweeted tweets) |
| Facet              | Hashtag used in tweet (\#)           |

| **Mention Slicer** |                                      |
|--------------------|--------------------------------------|
| **Column Name**    | **Description**                      |
| Tweet Id           | Tweet Id (includes retweeted tweets) |
| Facet              | Mention used in tweet (@)            |

| **Minimum Tweets**    |                                                                                                                   |
|-----------------------|-------------------------------------------------------------------------------------------------------------------|
| **Column Name**       | **Description**                                                                                                   |
| Minimum Tweets        | Values ranging from 1-100 (used in a slicer, these can define the minimum number of tweets to consider on a page) |
| *MinTweetsToConsider* | *DAX measure used to filter charts based on the value of ‘Minimum Tweets’*                                        |

| **Original Tweets**         |                                                                                                                                                                                                           |
|-----------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Column Name**             | **Description**                                                                                                                                                                                           |
| Master Id                   | Tweet Id (stores each individual tweet - no retweets)                                                                                                                                                     |
| Mentions                    | Currently not used                                                                                                                                                                                        |
| Hashtags                    | Currently not used                                                                                                                                                                                        |
| Tweet                       | Tweet text                                                                                                                                                                                                |
| Twitter Handle              | Handle of the tweet author                                                                                                                                                                                |
| User Number                 | Currently not used                                                                                                                                                                                        |
| Sentiment                   | Sentiment of tweet (measured on scale of -1 to 1 where -1 is very negative and 1 is very positive)                                                                                                        |
| Sentiment Bin               | Sentiment group a tweet falls in (e.g. a tweet with sentiment 0.36 would fall into group 0.3, a tweet with sentiment -0.22 would fall into group -0.2                                                     |
| Sentiment Positive/Negative | Tweets with sentiment 0 get put in the 'neutral' category, those above 0 get a positive group and below 0 get a negative grouping                                                                         |
| Language                    | Language tweet is in                                                                                                                                                                                      |
| Account Tag                 | Default value is unknown, if the tweet is matched to a tracked Twitter handle (e.g. tweeted from account or the Twitter handle is mentioned in the tweet) then the account tag is updated to that account |

| **Tweets**        |                                                                                                                        |
|-------------------|------------------------------------------------------------------------------------------------------------------------|
| **Column Name**   | **Descrpition**                                                                                                        |
| *%ofUniqueTweets* | *The percentage of original tweets *                                                                                   |
| Author Image URL  | A URL to the image of the author of the tweet                                                                          |
| Date              | Tweet date                                                                                                             |
| Direction         | Tweet direction (the different directions are outlined in the document)                                                |
| Favorited         | Currently not used                                                                                                     |
| Hours             | Time of tweet rounded to the nearest hour                                                                              |
| Image URL         | If images were included with the tweet this is the URL to the first image attached                                     |
| *isLast7Days*     | *Calculated column: Flag of whether a tweet occurred in the last 7 days*                                               |
| Latitude          | Currently not used                                                                                                     |
| *Link*            | *Link to tweet*                                                                                                        |
| Longitude         | Currently not used                                                                                                     |
| Master Id         | Tweet Id for original tweets (each master ID can be linked to multiple Tweet IDs)                                      |
| Minutes           | Time of tweet rounded to the nearest minute                                                                            |
| Original Date     | Time of tweet                                                                                                          |
| Retweet           | Currently not used                                                                                                     |
| Tweet Id          | Tweet ID (includes IDs for retweets)                                                                                   |
| User Favorites    | How many things did the author favorite                                                                                |
| User Followers    | How many followers does the tweet author have                                                                          |
| User Friends      | How many friends does the tweet author have                                                                            |
| User Number       | Currently not used                                                                                                     |
| User Total Tweets | How many tweets did the tweet author tweet                                                                             |
| Username          | Twitter handle of the author (if the tweet is a retweet then the author is the retweet author not the original poster) |

Report Walkthrough
------------------

The following section walks through each report page outlining the intent of the page. Throughout the Twitter reports the template will often reference the ‘direction’ of a tweet. As a reminder, here is the outline of the direction definitions:

inbound: I am tweeting ‘@Microsoft’/to:Microsoft

outbound: Tweet by ‘Microsoft’

inboundRT: I am retweeting someone else’s ‘@Microsoft’/to:Microsoft tweet

outboundRT: ‘Microsoft’ is retweeting my tweet

inboundReply: I am replying to a tweet from Microsoft

outboundReply: ‘Microsoft’ is replying to a tweet I tweeted ‘@Microsoft’/to:Microsoft

inboundReplyRT: I am retweeting a tweet that was someone tweeted @Microsoft/to:‘Microsoft’

RTofoutbound: I am retweeting a tweet that was originally posted by ‘Microsoft’

Hashtag: I am using \#Microsoft in the tweet

HashtagRT: I am retweeting a tweet that uses \#Microsoft

Text: I am tweeting with Microsoft mentioned in the tweet text (not @Microsoft or \#Microsoft)

RTText: I am retweeting a tweet that mentions Microsoft in the tweet text (not @Microsoft or \#Microsoft)

### 
Outbound Tweets:

If you are tracking any accounts, you will be able to see all the tweets and retweets tweeted **from** that account here. You will also see any retweets of the outbound tweets from external. You will be able to see how many outbound tweets occurred by tweet direction across time, what are the top trending tweets (based on the number of retweets) as well as high level statistics like average sentiment and the total followers impacted.

**Please note that if you are not tracking Twitter handles this page will be empty.**

![Image](Resources/media/image39.png)

### Inbound Tweets:

If you are tracking any accounts, you will be able to see all the tweets and retweets tweeted **to** that account here. You will be able to see how many inbound tweets occurred by tweet direction across time, what are the top trending tweets (based on the number of retweets) as well as high level statistics like average sentiment and the total followers impacted.

**Please note that if you are not tracking Twitter handles this page will be empty.**

![Image](Resources/media/image40.png)

### Author Hashtag Graph

This page allows you to explore a network graph of the hashtags that are being discussed on Twitter as well as how the hashtags are connected to each other. The dark nodes represent hashtags and colored nodes represent the authors.

-   A connection between a hashtag and author indicated the author’s tweet(s) contained that hashtag.

-   Multiple connections from an author to hashtag indicates the author is talking about many hashtags.

-   A hashtag that is connected to many authors indicates lots of people are talking about a specific hashtag

The bottom graph shows the volume of tweets coming in across time. This can also be used as a slicer to zoom into a specific time period.

You can also see which authors are most influential in your network i.e. tweeting systematically.

![Image](Resources/media/image41.png)

### Pivoting Overview

The pivoting overview page allows you to drill into a combination of authors, hashtags and tweet direction to focus on specific areas of interest. For example, if I notice a combination of hashtags trending on the network page I can select those to see what specific tweets are being tweeted. I can also get some quick, important statistics about those tweets such as a sentiment overview as well as the ratio of retweets to original tweets.

![Image](Resources/media/image42.png)

### Sentiment Analysis

This page allows you to monitor the sentiment of your tweets. The left hand side lets you quickly identify your top fans/haters or topics of concern/excitement. The report displays sentiment on the x axis and the volume of tweets on the y axis. Tweets are grouped by either authors (top left hand corner) or hashtags (middle left hand side). Tweets appearing in the top left hand corner of the graph are a cause of concern (tweets that are consistently negative) whereas top right hand corner tweets indicate lots of positivity.

You can also drill into a specific sentiment bin to quickly isolate e.g. only the most negative tweets or only the neutral tweets. You can also see how sentiment changes across time.

Finally, the top right hand corner has a slicer that lets you define the minimum number of tweets you want to see on the screen. For example, if you want to see only authors who have tweeted at least 3 tweets you would set the slicer value to 3.

![Image](Resources/media/image43.png)

### High Impact

The aim of this page is to help users monitor the most influentialtTweets. A user can use the ‘Table Sorter’ visual to define which variables it considers to define an influential tweet (by default 6 variables are selected). This is explained in further detail underneath the visual. A user can also do things like define the weighting of a variable (e.g. if sentiment is extremely important to me, I can give sentiment a weighting of 4 which will resort the table). All variables are normalized so they operatore on the same scale from 0-100.

Clicking on a tweet will cross filter the ‘influence’ stats on the left hand side to just the selected tweets (i.e. the sentiment, number of retweets etc). Here is an explanation of the default 6 variables:

-   **Sentiment:** The sentiment of the tweet

-   **Reteweets:** The number of times the tweet was retweeted

-   **User Favorites:** The sum of all the tweets users who tweeted + retweeted the tweet favorited

-   **Total User Tweets:** The sum of all the tweets users who tweeted + retweeted the tweet tweeted

-   **Friends Impacted:** The sum of all the friends of the users who tweeted + retweeted the tweet

-   **Followers Impacted:** The sum of all the followers of the users who tweeted + retweeted the tweet

![Image](Resources/media/image44.png)

Customizations
==============

Updating the Solution
---------------------

Once you set up the solution template you may want to modify the search terms or accounts you are following.

If you want to change your search terms you will need to log into your Azure portal account and open your Logic App. Here is an example of what a possible Logic App could look like:

![Image](Resources/media/image15.png)

If you wanted to modify your search query you should edit the text in the **Search Text** box and save your changes.

You might also want to change the accounts (Twitter Handles) you are following. In order to do this, you will need to go into your SQL database and open the configuration table. It should look something like this:

![Image](Resources/media/image45.png)

In order to change the accounts followed you will need to modify the ‘twitterHandle’ values inside the table (row 3) as well as the ‘twitterHandleId’ value (row 4).

Row 3 can be modified by appending/deleting any of the twitter handles you are interested (please make sure they are valid handles).

Row 4 requires the corresponding ID of the added twitter handle. You can check what a corresponding ID of a twitter handle is over [here](https://tweeterid.com/).

If you are including more than one Twitter Handle ID **please make sure they are comma separated without spaces in between (like the image above)**.

Extension Suggestions
---------------------

One of the great advantages of the template is that the solution is fully customizable! We encourage you to experiment and explore new ideas on how you can make the template more specific to your needs. Here are some ideas to get started:

### Bring in additional data sources: 

Since the application stores tweets in Azure SQL you can go ahead and bring in new data sources you might want to report on as well. Get a complete picture of social media by bringing in [LinkedIn](https://developer.linkedin.com/#!), [Facebook](https://developers.facebook.com/) or [Instagram](https://www.instagram.com/developer/) data.

From a more enterprise perspective, a suggestion would be to correlate the template with your CRM data. If you are tracking CRM campaigns, or selling products you could e.g. see how your campaigns are being perceived on social media or if a drop in product sales might be linked to some negative sentiment.

### Customize your Azure Function:

The Python script inside the Azure Function is completely customizable. You can go ahead and add additional enrichments to the data. For example, if there are specific themes you are interested in you could check if any of the tweets found correspond to those themes (e.g. if I had Power BI as my search term, I could define themes like ‘price’, ‘quality’, ‘data size’ and see if the incoming tweets contain any of those words). I could then add a column to SQL to document which theme was found and use that as a slicer inside my reports.

Other examples could be to add [Cognitive APIs](https://azure.microsoft.com/en-us/services/cognitive-services/text-analytics/) to do things like topic or keyword extraction on top of the twitter data.

Pricing
=======

Here is an estimate of the Azure costs (Logic App, Azure Function, Azure SQL, App Service Plan) based on the number of tweets processed:

Processing 10K tweets a month will cost approximately $60

Processing 50K tweets a month will cost approximately $165

Processing 100K tweets a month will cost approximately $235

Please keep in mind these are **estimated costs.** For a more detailed breakdown of the various components please refer to the [Azure calculator](https://azure.microsoft.com/en-us/pricing/calculator/) and select Logic App, Azure Function, Azure SQL and App Service. You can tweak all the options to see what the costs will look like and what modifications may suit your needs best.

The following defaults are set for you in the template (you can modify any of these after things get set up):

-   Azure SQL: Standard S1

-   App Service Plan: Dynamic

-   Logic Apps (trigger set for every 3 minutes)

-   Azure Functions (1536MB memory size)

For example, if you know you will be processing very few tweets a month, you could change the SQL Server from S1 to Basic. In that case you could bring down the costs of processing 10K tweets a month from about $60 to about $30.

Similarly, if you know you will be processing more than 100K tweets a month you may want to upgrade your App Service Plan from Standard to Premium which lets you process more Logic App actions each month.

Whilst the default setting should cater to most twitter template requirements, we encourage you to familiarize yourself with the various pricing options and tweak things to suit your needs.
