# TransactionManagerTz


# Description:
Create an API service for managing transactions. Allow the user to import a list of transactions from a CSV file, retrieve a list of transactions for specified dates, and export transactions to an Excel file.

# Functionality:

1. After uploading the file, .NET should process the content and add data to the database by transaction_id, which should be present in the CSV file. In other words, if there is no record in the database with such a unique transaction_id, add such a record to the database, and if such a record is present – update the transaction status.
2. The client's and transaction's time zone can be obtained from the location coordinates. Any libraries or online services can be used for such conversion.
3. Upon an export to Excel request, download a file with transaction information for the user (columns are at the discretion of the executor).
4. Allow the user to get a list of transactions for a range of two dates that occurred in the time zone of the current user making the API requests.
5. Allow the user to get a list of transactions for a range of two dates that occurred in the clients' time zones. The client's time zone is saved for each transaction and was obtained from the geolocation of the specific transaction.
6. Allow the user to get a list of transactions for January 2024 that occurred in the clients' time zones.

# Requirements:

1. Do not use automappers.
2. Create an informational page using Swagger for testing the API and document it.
3. Use Entity Framework for database migrations or another migrator of the executor's choice.
4. Do not use UnitOfWork and/or Repository pattern.
5. Upload the project to GitHub.
6. Database queries must be executed through SQL queries. Dapper can be used.
7. All comments, code, and general information in the project should be in English.
8. Use IANA time zones. Do not use Windows time zones.

## Task explanations:

There is no need to take the user's IP address and determine the time zone from it. You can pass it in the API request itself.
Each transaction in the file has the local time of the client when it was created and the time zone in which it was created.
The current user's API time zone includes transactions that occurred worldwide but are filtered within the time frame, for example, of Kyiv +2. At this moment, the API user is in Kyiv. For instance, if a transaction was created on December 31 in London, but at that moment it was January 1 in Kyiv, we display this transaction for the month of January.

## Run:
Run `docker compose up` in the root directory of the project.
