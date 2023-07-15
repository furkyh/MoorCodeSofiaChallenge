
# MoorCodeSofiaChallenge

User task list test project

## Table of Contents

- [user](#users data table)
- [user_task](#users' tasks data table)

## Description

- It is an application where users' tasks are saved, new task can be created, you can update, delete and filter by all fields.
- ASP.net Core MVC architecture is used in this project
- ASP.Net Core 7.0 is used as BackEnd
- Razor and Devextreme are used as FrontEnd
- With the Frkn javascript object and systemController I wrote in site.js under wwwroot, I have connected the FronEnd and BackEnd with the API I created.
- The Core project and the DataBase access layer were separated.

## Features

-Open a new task to users
-Updating tasks
-Deleting Tasks
-Filter by date/all fields


## Requirements

- .NET Core SDK [7.0]
- SQL Server

## Installation

1. Clone the repository: `git clone https://github.com/furkyh/MoorCodeSofiaChallenge.git`
2. Navigate to the project directory: `cd your-project`
3. Install the database with the sql script from the project (The SQL script is in the main directory, You can create the database using this script)
4. Open the project from solution
5. Inside the project, there is a Connection string in the staticdata class under the model folder. you can replace it with your own connection script
6. You can use the project by starting it in debug mode

## Usage

1. Start the project in debug mode
2. Open your browser and navigate to `http://localhost:5000` or `https://localhost:5001`.

Make sure you have done the installation steps before doing this!
