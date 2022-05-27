﻿using EmployeeDetails.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDetails.Common
{
    public class AzureCosmoDBActivity
    {
        private static readonly string EndpointUri = "https://localhost:8081";

        private static readonly string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private CosmosClient cosmosClient;

        private Database database;

        private Container container;

        private string databaseId = "MyLearning";

        private string containerId = "Employee";
        internal Task<Employee> objEmployeeDetails;

        public List<Employee> EmployeeId { get; private set; }
        public ItemResponse<Employee> EmployeeResponse { get; private set; }

        public async Task InitiateConnection()
        {
            // Create a new instance of the Cosmos Client 
            //configuring Azure Cosmosdb sql api details
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            await CreateDatabaseAsync();
            await CreateContainerAsync();
        }

        private async Task CreateDatabaseAsync()
        {
            // Create a new database
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
        }
        private async Task CreateContainerAsync()
        {
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/EmployeeId");
        }
        public async Task<ItemResponse<Employee>> SaveNewEmployeeItem(Employee objEmployee)
        {
            ItemResponse<Employee> employeeResponse = null;
            try
            {
                employeeResponse = await this.container.CreateItemAsync<Employee>(objEmployee, new PartitionKey(objEmployee.EmployeeGuid));
            }
            catch (CosmosException ex)
            {
                throw ex;
            }
            return employeeResponse;
        }
        public async Task<ItemResponse<Employee>> ModifyEmployeeItem(Employee objEmployee)
        {
            ItemResponse<Employee> employeeResponse = null;
            try
            {
                EmployeeResponse = await this.container.ReplaceItemAsync<Employee>(objEmployee, objEmployee.EmployeeGuid, new PartitionKey(objEmployee.EmployeeGuid));
            }
            catch (CosmosException ex)
            {
                throw ex;
            }
            return EmployeeResponse;
        }
        public async Task<ItemResponse<Employee>> GetEmployeeItem(string EmployeeId, string partionKey)
        {
            ItemResponse<Employee> EmployeeResponse = null;
            try
            {
                EmployeeResponse = await this.container.ReadItemAsync<Employee>(EmployeeId, new PartitionKey(partionKey));
            }
            catch (CosmosException ex)
            {
                throw ex;
            }
            return EmployeeResponse;
        }
        public async Task<List<Employee>> GetAllEmployees()
        {
            var sqlQueryText = "SELECT * FROM c";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Employee> queryResultSetIterator = this.container.GetItemQueryIterator<Employee>(queryDefinition);

            List<Employee> lstEmployees = new List<Employee>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Employee> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                lstEmployees = currentResultSet.Select(r => new Employee()
                {
                    Name = r.Name,
                    DateOfBirth = r.DateOfBirth,
                    PhoneNo = r.PhoneNo,
                    Email = r.Email,
                    EmployeeGuid = r.EmployeeGuid,
                }).ToList();

            }
            return lstEmployees;
        }
    }
}
