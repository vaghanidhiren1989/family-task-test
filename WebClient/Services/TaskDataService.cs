﻿using Domain.Commands;
using Domain.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebClient.Abstractions;
using Microsoft.AspNetCore.Components;
using Domain.ViewModel;
using Core.Extensions.ModelConversion;

namespace WebClient.Services
{
    public class TaskDataService : ITaskDataService
    {

        private readonly HttpClient _httpClient;
        public TaskDataService(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("FamilyTaskAPI");
            tasks = new List<TaskVm>();
            LoadTasks();
        }
        private IEnumerable<TaskVm> tasks;

        public IEnumerable<TaskVm> Tasks => tasks;

        public TaskVm SelectedTask { get; private set; }

        public event EventHandler TasksChanged;
        public event EventHandler SelectedTaskChanged;
        public event EventHandler<string> CreateTaskFailed;

        private async void LoadTasks()
        {
            tasks = (await GetAllTasks()).Payload;
            TasksChanged?.Invoke(this, null);
        }

        public async Task<CreateTaskCommandResult> Create(CreateTaskCommand command)
        {
            return await _httpClient.PostJsonAsync<CreateTaskCommandResult>("tasks", command);
        }

        public async Task<GetAllTasksQueryResult> GetAllTasks()
        {
            return await _httpClient.GetJsonAsync<GetAllTasksQueryResult>("tasks");
        }

        public async Task<UpdateTaskCommandResult> Update(UpdateTaskCommand command)
        {
            return await _httpClient.PutJsonAsync<UpdateTaskCommandResult>("tasks/" + command.Id, command);
        }

        public async Task CreateTask(TaskVm model)
        {
            var result = await Create(model.ToCreateTaskCommand());
            if (result != null)
            {
                var updatedList = (await GetAllTasks()).Payload;

                if (updatedList != null)
                {
                    tasks = updatedList;
                    TasksChanged?.Invoke(this, null);
                    return;
                }
                CreateTaskFailed?.Invoke(this, "The creation was successful, but we can no longer get an updated list of tasks from the server.");
            }

            CreateTaskFailed?.Invoke(this, "Unable to create record.");
        }
    }
}