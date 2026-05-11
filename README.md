# Finance Tracker
Finance tracker is an application designed to make it easier to track family finances.
This project is exactly the server part.

## Why?
* In everyday life - to track family finances, to understand what amount you can count on at a particular moment.
* As studing project - to improve C# knowleges, including learning ASP.NET Core, to learn REST API development and API interaction in common, to become familiar with all the necessary related technologies and tools, to consolidate knowleges and patterns of developing.

## Used stack
* ASP.NET Core Web API - core.
* PostgeSQL - Data base managment system.
* Redis - Fast NoSQL for fail-safe cache.
* Docker - containering and deployment.

## Ready features
* Authorization and authentification throw email+password or telegram account, with following verification with JWT tokens and their updating using refresh tokens.
* Users grouping (by now group can take only 2 users, and one user can only be a member of one group).
* Tracking users incomes and expenses (which the user added), calculating current balance.
* Possibility to get statistical view of transactions with division into categories (by user and by group).

## Roadmap
* Add authorization with OAuth.
* To try to realize logic of multiple groups for one user, or/and increase count of users in one group.
* Improve safety and performance.
