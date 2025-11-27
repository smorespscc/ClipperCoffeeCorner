		Mattox Marsh - SMS / E-Mail & API Work

	Summary
-

	Concept
-

	Challenges
-

	What Worked Well & What Didn't
-

	Differences in Approach or Special Configuration
-

	Advice for Next Cohort
-


		Kyle Geissinger - Wait Estimation & Machine Learning

	Summary
I lucked out with a mostly stand-alone feature so my work was fairly straight-forward:
Prototyping with the ML.Net machine learning tools built into Visual Studio, which included preparing training data, training the model, and figuring out how to access it from the program.
To implement into the project, I had to understand the SQL database to access it and prepare training data, and understand how current Orders are stored while the program is running in order to send the correct "feature" data the machine learning model uses to predict a "label" for.
I created a lot of SQL scripts to replicate the project's database locally, generate a random set of random, completed dummy orders which other SQL scripts could use to generate training data.
Normally I could more work into improving the model but without any real data to compare to and train on it I can't do much more.

	Concept
The main idea is that the machine learning model can easily be re-trained. If there was a change in staff that affected the processing time of multiple different menu items in a way that would be difficulty to quantify by any algorithm or human, the model would be able to pick up on it somewhat automatically. With easy access to an SQL database of past orders, training data could easily be isolated by rush hours, slow mornings, staff schedules, or holidays.

	Challenges
The first challenge was moving from prototyping to the main repo. As the model and training data rely heavily on the way the database and ordering system are set up, it couldn't be implemented until they were.
The second challenge was accessing the SQL database. All I found in the repo was a single SQL script which prepared the database, so I had to set up SQL (through visual studio rather than my personal SQL servers) and get the coffee1.sql script working on my end.
The third challenge was creating training data. Because we have no actual order history, I had to use SQL language to try and generate plausible table entries and insert them. After sufficient dummy data was made, I had to process the dummy order history into training data for the ML model.
The fourth challenge was getting training data that matched the data passed to the wait time estimation method. Originally I wanted to use the code for the order, which includes the menu item AND all of its modifications. However, while in SQL I can easily grab data from 2 tables over, I can't do that as easily while the program is running. So I had to change my training data to merely have the ID of the menu item, and not care for modifications.
The fifth challenge was actually an unresolved bug in the ML Model Builder extension for Visual Studio. Currently, passing SQL data or even a CSV to the model builder results in an error. As a result I had to output the training data as a tab-demarketed txt file. https://github.com/dotnet/machinelearning-modelbuilder/issues/3005

	What Worked Well & What Didn't
The actual machine learning model worked remarkably well. It's built right into Visual Studio, and at least on my very fast computer it ran and trained very well. What didn't work well was how I communicated the needs of the wait time estimation method I suppose. I said all I needed to know was what the order consisted of, but I didn't end up getting access to the modifications made to them in a way that I could easily pass on to the ML Model.

	Differences in Approach or Special Configuration
I was confident in my ability to fit my feature in to other people's code, so after prototyping I was very passive. I felt like if I started communicating more specifically about the feature, I would end up telling people how to do their parts of the project. I already mentioned above how I had to use txt training data rather than accessing the SQL directly.

	Advice for Next Cohort
Have everyone work together on making a bunch of empty interfaces and just setting up all the internals, then have the features set up. Think of a very basic program, the minimal viable product, then add all the bells and whistles. Practice figuring out what is essential for the program, what things need to be available for ease of future expansion, and what are bonus features that can be added on top.