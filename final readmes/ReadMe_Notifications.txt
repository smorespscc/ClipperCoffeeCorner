		Mattox Marsh - SMS / E-Mail & API Work

	=== Summary ===
I worked on implementing SMS and Email notifications for orders when they are placed and when they are marked as completed. I also worked on an API for notifications 
and wait time estimation so that it would be easy to integrate with the rest of the project and to make our features more modular and "future-proof". Our notifications
and wait time estimation features are accessible through simple calls to the endpoints in NotificationsController and only require single parameters like order ID to function.

	=== Concept ===
The two main endpoints are OrderPlaced and OrderComplete in NotificationsController, and they are both pretty much the same except they call different methods in the 
WaitTimeEstimator service. The general flow is...

Call to NotificationsController 
	>>> Call to WaitTimeEstimationService
		> get order info from DB API
		> get detailed item info from DB API
		> call WaitTimeEstimator service with order info to get estimated wait time
		> get user info from DB API (if the order is associated with a userId)
		> call notification services with order and user info to send notifications

	=== Challenges ===
The main headache was getting SMS to work because of some new regulations that went into effect in 2024 that require verification and stuff to use SMS services. I was never
able to actually get verified to send actual SMS messages, so I opted for using Twilio instead of Azure for SMS because Twilio has a virtual phone you can use for testing.
Another kind of self-inflicted challenge was having to re-write a bunch of code to make it compatible with the data models the other teams were using. I should have established 
all that stuff first before doing a lot of the work on notifications, but I was impatient.

	=== What Worked Well & What Didn't ===
Everything with notifications works well because really, there isn't anything complicated going on. The main improvement I think could be made is trimming down all of our
DTOs and making all of the methods work with just 1 or 2 standard data models, because a lot of our data models have overlapping fields and stuff. Also the names of DTOs
and fields within them can be a little confusing, so I'd probably fix those too. Although that would have to be done with the other teams because we share the DTOs and stuff.

	=== Differences in Approach or Special Configuration ===
Setting up Sms and Email notifications is pretty straightforward but there are a few of steps. You gotta set up the Twilio and SendGrid accounts and then fill the
necessary fields in appsettings.json with the keys and tokens and stuff for your accounts.

1. Set up Twilio account with their guide - https://www.twilio.com/docs/messaging/quickstart
	- only need to do the "sign up and get a number" part
	- collect your Account SID, Auth Token, and trial phone number
	- trial number does not have to be verified to use the virtual phone found in Account Dashboard > Develop > Messaging > Try it out > Send an SMS

2. Set up SendGrid account
	- In Twilio dashboard, click "Open SendGrid Console" in Develop > Email > Overview
	- Set up SendGrid account (create a "From"" email and stuff and verify it) (it yells at you for using @gmail but you can ignore it)
	- collect your API Key, verified "From" email address, and "From" name

3. Fill in appsettings.json with the relevant information.
	- Fill in all Twilio and SendGrid fields
	- Also fill in "NotificationTestDetails" with the email and number you want to send to for testing purposes
	- The "ToPhoneNumber" should be the number of the virtual phone in your Twilio dashboard which can be found in Account Dashboard > Develop > Messaging > Try it out > Send an SMS

4. Run test for notifications and I guess also WaitTimeEstimator while you're at it
	- build and run the project locally
	- open windows CMD and type the following commands to run the testing endpoints in NotificationsController
		- curl -X POST "https://localhost:xxxx/api/notifications/test-notifications?notificationPref=Email" -k
		- curl -X POST "https://localhost:xxxx/api/notifications/test-wait-time" -k
		*replace "xxxx" with whatever your port is and change "Email" at the end of the first command to "Sms" to test SMS*

	=== Advice for Next Cohort ===
What Kyle said vvv


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