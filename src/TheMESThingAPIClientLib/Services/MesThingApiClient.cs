using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using TheMESThingAPIClientLib.Models;
using TheMESThingAPIClientLib.Models.Mes;
using TheMESThingAPIClientLib.Models.M365;

namespace TheMESThingAPIClientLib.Services;

public class MesThingApiClient(HttpClient http)
{
    static readonly JsonSerializerOptions Opts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // ── helpers ──────────────────────────────────────────────────────────────
    async Task<PagedResult<T>> GetPagedAsync<T>(string url) =>
        (await http.GetFromJsonAsync<PagedResult<T>>(url, Opts))!;

    async Task<T?> GetAsync<T>(string url) =>
        await http.GetFromJsonAsync<T>(url, Opts);

    async Task<T> PostAsync<T>(string url, T body)
    {
        var r = await http.PostAsJsonAsync(url, body, Opts);
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<T>(Opts))!;
    }

    async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest body)
    {
        var r = await http.PostAsJsonAsync(url, body, Opts);
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<TResponse>(Opts))!;
    }

    async Task<T> PutAsync<T>(string url, T body)
    {
        var r = await http.PutAsJsonAsync(url, body, Opts);
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<T>(Opts))!;
    }

    async Task DeleteAsync(string url)
    {
        var r = await http.DeleteAsync(url);
        r.EnsureSuccessStatusCode();
    }

    async Task<IReadOnlyList<T>> GetListAsync<T>(string url) =>
        (await http.GetFromJsonAsync<IReadOnlyList<T>>(url, Opts))!;

    async Task PostVoidAsync<T>(string url, T body)
    {
        var r = await http.PostAsJsonAsync(url, body, Opts);
        r.EnsureSuccessStatusCode();
    }

    async Task PutVoidAsync<T>(string url, T body)
    {
        var r = await http.PutAsJsonAsync(url, body, Opts);
        r.EnsureSuccessStatusCode();
    }

    async Task PutVoidAsync(string url)
    {
        var r = await http.PutAsync(url, null);
        r.EnsureSuccessStatusCode();
    }

    async Task<byte[]?> GetBytesAsync(string url)
    {
        var r = await http.GetAsync(url);
        if (r.StatusCode == HttpStatusCode.NotFound) return null;
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadAsByteArrayAsync();
    }

    // ════════════════════════════════════════════════════════════════════════
    // MES
    // ════════════════════════════════════════════════════════════════════════

    // Customers
    public Task<PagedResult<Customer>> GetCustomersAsync(Guid? tenantId = null, int page = 1, int pageSize = 20) =>
        GetPagedAsync<Customer>($"api/mes/customers?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}");
    public Task<Customer?> GetCustomerAsync(Guid id) => GetAsync<Customer>($"api/mes/customers/{id}");
    public Task<Customer> CreateCustomerAsync(Customer c) => PostAsync("api/mes/customers", c);
    public Task<Customer> UpdateCustomerAsync(Guid id, Customer c) => PutAsync($"api/mes/customers/{id}", c);
    public Task DeleteCustomerAsync(Guid id) => DeleteAsync($"api/mes/customers/{id}");

    // Departments
    public Task<PagedResult<Department>> GetDepartmentsAsync(Guid? tenantId = null, int page = 1, int pageSize = 20) =>
        GetPagedAsync<Department>($"api/mes/departments?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}");
    public Task<Department?> GetDepartmentAsync(Guid id) => GetAsync<Department>($"api/mes/departments/{id}");
    public Task<Department> CreateDepartmentAsync(Department d) => PostAsync("api/mes/departments", d);
    public Task<Department> UpdateDepartmentAsync(Guid id, Department d) => PutAsync($"api/mes/departments/{id}", d);
    public Task DeleteDepartmentAsync(Guid id) => DeleteAsync($"api/mes/departments/{id}");

    // ProductionLines
    public Task<PagedResult<ProductionLine>> GetProductionLinesAsync(Guid? tenantId = null, int page = 1, int pageSize = 20) =>
        GetPagedAsync<ProductionLine>($"api/mes/production-lines?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}");
    public Task<ProductionLine?> GetProductionLineAsync(Guid id) => GetAsync<ProductionLine>($"api/mes/production-lines/{id}");
    public Task<ProductionLine> CreateProductionLineAsync(ProductionLine l) => PostAsync("api/mes/production-lines", l);
    public Task<ProductionLine> UpdateProductionLineAsync(Guid id, ProductionLine l) => PutAsync($"api/mes/production-lines/{id}", l);
    public Task DeleteProductionLineAsync(Guid id) => DeleteAsync($"api/mes/production-lines/{id}");

    // Machines
    public Task<PagedResult<Machine>> GetMachinesAsync(Guid? tenantId = null, int page = 1, int pageSize = 20) =>
        GetPagedAsync<Machine>($"api/mes/machines?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}");
    public Task<Machine?> GetMachineAsync(Guid id) => GetAsync<Machine>($"api/mes/machines/{id}");
    public Task<Machine> CreateMachineAsync(Machine m) => PostAsync("api/mes/machines", m);
    public Task<Machine> UpdateMachineAsync(Guid id, Machine m) => PutAsync($"api/mes/machines/{id}", m);
    public Task DeleteMachineAsync(Guid id) => DeleteAsync($"api/mes/machines/{id}");

    // Skills
    public Task<PagedResult<Skill>> GetSkillsAsync(Guid? tenantId = null, int page = 1, int pageSize = 20) =>
        GetPagedAsync<Skill>($"api/mes/skills?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}");
    public Task<Skill?> GetSkillAsync(Guid id) => GetAsync<Skill>($"api/mes/skills/{id}");
    public Task<Skill> CreateSkillAsync(Skill s) => PostAsync("api/mes/skills", s);
    public Task<Skill> UpdateSkillAsync(Guid id, Skill s) => PutAsync($"api/mes/skills/{id}", s);
    public Task DeleteSkillAsync(Guid id) => DeleteAsync($"api/mes/skills/{id}");

    // Operators
    public Task<PagedResult<Operator>> GetOperatorsAsync(Guid? tenantId = null, int page = 1, int pageSize = 20) =>
        GetPagedAsync<Operator>($"api/mes/operators?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}");
    public Task<Operator?> GetOperatorAsync(Guid id) => GetAsync<Operator>($"api/mes/operators/{id}");
    public Task<Operator> CreateOperatorAsync(Operator o) => PostAsync("api/mes/operators", o);
    public Task<Operator> UpdateOperatorAsync(Guid id, Operator o) => PutAsync($"api/mes/operators/{id}", o);
    public Task DeleteOperatorAsync(Guid id) => DeleteAsync($"api/mes/operators/{id}");

    // Shifts
    public Task<PagedResult<Shift>> GetShiftsAsync(Guid? tenantId = null, int page = 1, int pageSize = 20) =>
        GetPagedAsync<Shift>($"api/mes/shifts?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}");
    public Task<Shift?> GetShiftAsync(Guid id) => GetAsync<Shift>($"api/mes/shifts/{id}");
    public Task<Shift> CreateShiftAsync(Shift s) => PostAsync("api/mes/shifts", s);
    public Task<Shift> UpdateShiftAsync(Guid id, Shift s) => PutAsync($"api/mes/shifts/{id}", s);
    public Task DeleteShiftAsync(Guid id) => DeleteAsync($"api/mes/shifts/{id}");

    // Products
    public Task<PagedResult<Product>> GetProductsAsync(Guid? tenantId = null, int page = 1, int pageSize = 20) =>
        GetPagedAsync<Product>($"api/mes/products?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}");
    public Task<Product?> GetProductAsync(Guid id) => GetAsync<Product>($"api/mes/products/{id}");
    public Task<Product> CreateProductAsync(Product p) => PostAsync("api/mes/products", p);
    public Task<Product> UpdateProductAsync(Guid id, Product p) => PutAsync($"api/mes/products/{id}", p);
    public Task DeleteProductAsync(Guid id) => DeleteAsync($"api/mes/products/{id}");

    // WorkOrders
    public Task<PagedResult<WorkOrder>> GetWorkOrdersAsync(Guid? tenantId = null, string? status = null, int page = 1, int pageSize = 20) =>
        GetPagedAsync<WorkOrder>($"api/mes/work-orders?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}{(status != null ? $"&status={status}" : "")}");
    public Task<WorkOrder?> GetWorkOrderAsync(Guid id) => GetAsync<WorkOrder>($"api/mes/work-orders/{id}");
    public Task<WorkOrder> CreateWorkOrderAsync(WorkOrder w) => PostAsync("api/mes/work-orders", w);
    public Task<WorkOrder> UpdateWorkOrderAsync(Guid id, WorkOrder w) => PutAsync($"api/mes/work-orders/{id}", w);
    public Task DeleteWorkOrderAsync(Guid id) => DeleteAsync($"api/mes/work-orders/{id}");

    // ProductionOrders
    public Task<PagedResult<ProductionOrder>> GetProductionOrdersAsync(Guid? tenantId = null, string? status = null, int page = 1, int pageSize = 20) =>
        GetPagedAsync<ProductionOrder>($"api/mes/production-orders?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}{(status != null ? $"&status={status}" : "")}");
    public Task<ProductionOrder?> GetProductionOrderAsync(Guid id) => GetAsync<ProductionOrder>($"api/mes/production-orders/{id}");
    public Task<ProductionOrder> CreateProductionOrderAsync(ProductionOrder p) => PostAsync("api/mes/production-orders", p);
    public Task<ProductionOrder> UpdateProductionOrderAsync(Guid id, ProductionOrder p) => PutAsync($"api/mes/production-orders/{id}", p);
    public Task DeleteProductionOrderAsync(Guid id) => DeleteAsync($"api/mes/production-orders/{id}");

    // ════════════════════════════════════════════════════════════════════════
    // M365 – Email
    // ════════════════════════════════════════════════════════════════════════

    public Task<IReadOnlyList<Email>> GetInboxEmailsAsync(string userId, int maxResults = 50) =>
        GetListAsync<Email>($"api/m365/email/{userId}/inbox?maxResults={maxResults}");
    public Task<IReadOnlyList<Email>> GetEmailsAsync(string userId, int maxResults = 50) =>
        GetListAsync<Email>($"api/m365/email/{userId}/messages?maxResults={maxResults}");
    public Task<IReadOnlyList<MailFolder>> GetMailFoldersAsync(string userId) =>
        GetListAsync<MailFolder>($"api/m365/email/{userId}/folders");
    public Task<IReadOnlyList<Email>> GetEmailsByFolderAsync(string userId, string folderId, int maxResults = 50) =>
        GetListAsync<Email>($"api/m365/email/{userId}/folders/{folderId}/messages?maxResults={maxResults}");
    public Task<Email?> GetEmailAsync(string userId, string messageId) =>
        GetAsync<Email>($"api/m365/email/{userId}/messages/{messageId}");
    public Task<Email> SendEmailAsync(string userId, NewEmail body) =>
        PostAsync<NewEmail, Email>($"api/m365/email/{userId}/messages", body);
    public Task SendEmailAndSaveAsync(string userId, NewEmail body) =>
        PostVoidAsync($"api/m365/email/{userId}/messages/send-and-save", body);
    public Task MarkEmailAsReadAsync(string userId, string messageId) =>
        PutVoidAsync($"api/m365/email/{userId}/messages/{messageId}/read");
    public Task DeleteEmailAsync(string userId, string messageId) =>
        DeleteAsync($"api/m365/email/{userId}/messages/{messageId}");

    // ════════════════════════════════════════════════════════════════════════
    // M365 – Calendar
    // ════════════════════════════════════════════════════════════════════════

    public Task<IReadOnlyList<CalendarEvent>> GetCalendarEventsAsync(string userId, DateTimeOffset? start = null, DateTimeOffset? end = null, int maxResults = 50)
    {
        var url = $"api/m365/calendar/{userId}/events?maxResults={maxResults}";
        if (start.HasValue) url += $"&start={Uri.EscapeDataString(start.Value.ToString("o"))}";
        if (end.HasValue)   url += $"&end={Uri.EscapeDataString(end.Value.ToString("o"))}";
        return GetListAsync<CalendarEvent>(url);
    }
    public Task<CalendarEvent?> GetCalendarEventAsync(string userId, string eventId) =>
        GetAsync<CalendarEvent>($"api/m365/calendar/{userId}/events/{eventId}");
    public Task<CalendarEvent> CreateCalendarEventAsync(string userId, NewM365CalendarEvent body) =>
        PostAsync<NewM365CalendarEvent, CalendarEvent>($"api/m365/calendar/{userId}/events", body);
    public Task UpdateCalendarEventAsync(string userId, string eventId, UpdateCalendarEvent body) =>
        PutVoidAsync($"api/m365/calendar/{userId}/events/{eventId}", body);
    public Task DeleteCalendarEventAsync(string userId, string eventId) =>
        DeleteAsync($"api/m365/calendar/{userId}/events/{eventId}");

    // ════════════════════════════════════════════════════════════════════════
    // M365 – Users
    // ════════════════════════════════════════════════════════════════════════

    public Task<IReadOnlyList<M365User>> GetM365UsersAsync(string? filter = null, int maxResults = 100)
    {
        var url = $"api/m365/users?maxResults={maxResults}";
        if (filter != null) url += $"&filter={Uri.EscapeDataString(filter)}";
        return GetListAsync<M365User>(url);
    }
    public Task<IReadOnlyList<M365User>> SearchM365UsersAsync(string query, int maxResults = 25) =>
        GetListAsync<M365User>($"api/m365/users/search?query={Uri.EscapeDataString(query)}&maxResults={maxResults}");
    public Task<M365User?> GetM365UserAsync(string userIdOrUpn) =>
        GetAsync<M365User>($"api/m365/users/{Uri.EscapeDataString(userIdOrUpn)}");
    public Task<M365User> CreateM365UserAsync(NewUser body) =>
        PostAsync<NewUser, M365User>("api/m365/users", body);
    public Task UpdateM365UserAsync(string userIdOrUpn, UpdateUser body) =>
        PutVoidAsync($"api/m365/users/{Uri.EscapeDataString(userIdOrUpn)}", body);
    public Task DisableM365UserAsync(string userIdOrUpn) =>
        PutVoidAsync($"api/m365/users/{Uri.EscapeDataString(userIdOrUpn)}/disable");
    public Task DeleteM365UserAsync(string userIdOrUpn) =>
        DeleteAsync($"api/m365/users/{Uri.EscapeDataString(userIdOrUpn)}");

    // ════════════════════════════════════════════════════════════════════════
    // M365 – Drive
    // ════════════════════════════════════════════════════════════════════════

    public Task<IReadOnlyList<DriveItem>> GetDriveRootItemsAsync(string userId) =>
        GetListAsync<DriveItem>($"api/m365/drive/{userId}/items");
    public Task<DriveItem?> GetDriveItemAsync(string userId, string itemId) =>
        GetAsync<DriveItem>($"api/m365/drive/{userId}/items/{itemId}");
    public Task<IReadOnlyList<DriveItem>> GetDriveItemChildrenAsync(string userId, string itemId) =>
        GetListAsync<DriveItem>($"api/m365/drive/{userId}/items/{itemId}/children");
    public Task<byte[]?> DownloadDriveFileAsync(string userId, string itemId) =>
        GetBytesAsync($"api/m365/drive/{userId}/items/{itemId}/download");
    public Task<DriveItem> CreateDriveFolderAsync(string userId, string parentItemId, string folderName) =>
        PostAsync<CreateFolderRequest, DriveItem>($"api/m365/drive/{userId}/items/{parentItemId}/folder", new CreateFolderRequest(folderName));
    public Task<DriveItem> UploadDriveFileAsync(string userId, string parentItemId, DriveUploadFile file) =>
        PostAsync<DriveUploadFile, DriveItem>($"api/m365/drive/{userId}/items/{parentItemId}/upload", file);
    public Task DeleteDriveItemAsync(string userId, string itemId) =>
        DeleteAsync($"api/m365/drive/{userId}/items/{itemId}");
}
