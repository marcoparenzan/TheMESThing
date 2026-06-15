// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  UTOPIA PLANITIA FLEET YARDS — MES Demo Data Seeder                     ║
// ║  Starfleet Manufacturing Command · Star Trek Conference Demo             ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using System.Net.Http.Headers;
using TheMESThingAPIClientLib.Models.Mes;
using TheMESThingAPIClientLib.Services;

// ── Configuration ─────────────────────────────────────────────────────────────
string baseUrl = args.Length > 0 ? args[0] : "https://localhost:7000";

var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

var api = new MesThingApiClient(http);

Banner("UTOPIA PLANITIA FLEET YARDS — Demo Seeder");
Ok($"Target API: {baseUrl}");
Console.WriteLine();

// ══════════════════════════════════════════════════════════════════════════════
// 4. CUSTOMERS (Starfleet divisions & allied civilizations)
// ══════════════════════════════════════════════════════════════════════════════
Section("4. Customers — Starfleet Divisions & Allied Orders");

var customerDefs = new[]
{
    ("CUST-SFO",  "Starfleet Operations",          "sfops@starfleet.fed",        "+1-415-000-0001", "Starfleet HQ, San Francisco, Earth",   "Federation"),
    ("CUST-SFEX", "Starfleet Exploration Division", "exploration@starfleet.fed",  "+1-415-000-0002", "Starfleet HQ, San Francisco, Earth",   "Federation"),
    ("CUST-ENT",  "USS Enterprise Command",         "enterprise@starfleet.fed",   "+1-415-000-1701", "Orbital Dock, McKinley Station",       "Federation"),
    ("CUST-DSN",  "Deep Space Nine Command",         "ds9@starfleet.fed",          "+1-415-000-9000", "Deep Space Nine, Bajoran Sector",      "Federation"),
    ("CUST-VOY",  "USS Voyager Mission Control",     "voyager@starfleet.fed",      "+1-415-000-7456", "Starfleet Command, Earth",             "Federation"),
    ("CUST-KDF",  "Klingon Defense Force",           "kdf@klingon.empire",         "+0-999-000-0001", "Qo'noS Industrial District",           "Klingon Empire"),
    ("CUST-ROM",  "Romulan Star Empire Procurement", "procurement@romulan.emp",    "+0-888-000-0001", "Romulus Central Command",              "Romulan Star Empire"),
    ("CUST-AND",  "Andorian Engineering Corps",      "aec@andoria.fed",            "+1-416-000-0001", "Andoria, Northern Hemisphere",         "Federation"),
    ("CUST-VUL",  "Vulcan Science Academy",          "vsa@vulcan.fed",             "+1-650-000-0001", "ShiKahr, Vulcan",                      "Federation"),
    ("CUST-MCK",  "McKinley Station Refit Ops",      "refit@mckinley.starfleet.fed","+1-415-000-3000","McKinley Station, Earth Orbit",        "Federation"),
};

var createdCustomers = new Dictionary<string, Customer>();
foreach (var (code, name, email, phone, addr, country) in customerDefs)
{
    var c = await api.CreateCustomerAsync(new Customer(
        Guid.Empty, tid, code, name, email, phone, addr, country,
        ExternalMicrosoft365Id: null, IsActive: true,
        CreatedAtUtc: DateTime.UtcNow, UpdatedAtUtc: DateTime.UtcNow,
        CreatedByUserId: null, UpdatedByUserId: null));
    createdCustomers[code] = c;
    Ok($"  Customer → {c.CustomerCode,-12} {c.CustomerName}");
}

// ══════════════════════════════════════════════════════════════════════════════
// 5. DEPARTMENTS
// ══════════════════════════════════════════════════════════════════════════════
Section("5. Departments — Utopia Planitia Fleet Yards");

//var managerUserId = createdUsers["entra-laforge-001"].AppUserId;

//var deptDefs = new (string Code, string Name, string? ParentCode, string CostCenter, Guid ManagerId)[]
//{
//    ("DEPT-WCA",  "Warp Core Assembly",         null, "CC-WCA-001", createdUsers["entra-laforge-001"].AppUserId),
//    ("DEPT-HUL",  "Hull Integration",           null, "CC-HUL-001", createdUsers["entra-riker-001"].AppUserId),
//    ("DEPT-SBO",  "Shuttle Bay Operations",     null, "CC-SBO-001", createdUsers["entra-chakotay-001"].AppUserId),
//    ("DEPT-TAC",  "Tactical Systems",           null, "CC-TAC-001", createdUsers["entra-worf-001"].AppUserId),
//    ("DEPT-EPS",  "EPS Grid & Power Systems",   null, "CC-EPS-001", createdUsers["entra-scotty-001"].AppUserId),
//    ("DEPT-REP",  "Replicator Manufacturing",   null, "CC-REP-001", createdUsers["entra-data-001"].AppUserId),
//    ("DEPT-QA",   "Federation Compliance & QA", null, "CC-QA-001",  createdUsers["entra-bashir-001"].AppUserId),
//    ("DEPT-MNT",  "Engineering Maintenance",    null, "CC-MNT-001", createdUsers["entra-obrien-001"].AppUserId),
//    ("DEPT-LCRS", "LCARS Analytics Center",     null, "CC-ANA-001", createdUsers["entra-dax-001"].AppUserId),
//    ("DEPT-CGO",  "Cargo Operations",           null, "CC-CGO-001", createdUsers["entra-quark-001"].AppUserId),
//};

//var createdDepts = new Dictionary<string, Department>();
//foreach (var (code, name, parentCode, costCenter, mgr) in deptDefs.AsEnumerable())
//{
//    Guid? parentId = parentCode != null && createdDepts.TryGetValue(parentCode, out var pd) ? pd.DepartmentId : null;
//    var d = await api.CreateDepartmentAsync(new Department(
//        Guid.Empty, tid, code, name, parentId, mgr, costCenter,
//        ExternalMicrosoft365Id: null, IsActive: true,
//        CreatedAtUtc: DateTime.UtcNow, UpdatedAtUtc: DateTime.UtcNow));
//    createdDepts[code] = d;
//    Ok($"  Department → {d.DepartmentCode,-12} {d.DepartmentName}");
//}

// ══════════════════════════════════════════════════════════════════════════════
// 6. PRODUCTION LINES
// ══════════════════════════════════════════════════════════════════════════════
Section("6. Production Lines");

var lineDefs = new[]
{
    ("LINE-WC01", "Warp Core Assembly Line Alpha",       "DEPT-WCA", "Assembly",    120.0, "Units/Day"),
    ("LINE-WC02", "Warp Core Assembly Line Beta",        "DEPT-WCA", "Assembly",     90.0, "Units/Day"),
    ("LINE-HUL01","Galaxy-Class Hull Integration Line",  "DEPT-HUL", "Integration",   8.0, "Hulls/Week"),
    ("LINE-HUL02","Intrepid-Class Hull Integration Line","DEPT-HUL", "Integration",  12.0, "Hulls/Week"),
    ("LINE-SHU01","Shuttlecraft Assembly Line 01",       "DEPT-SBO", "Assembly",     30.0, "Units/Day"),
    ("LINE-TAC01","Torpedo Bay Assembly Line",           "DEPT-TAC", "Assembly",     50.0, "Units/Day"),
    ("LINE-EPS01","EPS Conduit Fabrication Line",        "DEPT-EPS", "Fabrication", 200.0, "Meters/Day"),
    ("LINE-REP01","Replicator Unit Production Line",     "DEPT-REP", "Assembly",     80.0, "Units/Day"),
};

var createdLines = new Dictionary<string, ProductionLine>();
foreach (var (code, name, deptCode, type, cap, unit) in lineDefs)
{
    var l = await api.CreateProductionLineAsync(new ProductionLine(
        Guid.Empty, tid, createdDepts[deptCode].DepartmentId,
        code, name, type, cap, unit, IsActive: true,
        CreatedAtUtc: DateTime.UtcNow, UpdatedAtUtc: DateTime.UtcNow));
    createdLines[code] = l;
    Ok($"  Line → {l.LineCode,-14} {l.LineName}");
}

// ══════════════════════════════════════════════════════════════════════════════
// 7. MACHINES
// ══════════════════════════════════════════════════════════════════════════════
Section("7. Machines — Sensor Grid IoT Devices");

var machineDefs = new[]
{
    ("MCH-WC-CNC01", "Warp Coil CNC 01",           "DEPT-WCA", "LINE-WC01", "CNC",              "Starfleet Engineering", "WC-CNC-MK7", "SN-WC-CNC-001", 45.0,  80.0,  "IOT-WC-CNC-01", "Connected"),
    ("MCH-WC-CNC02", "Warp Coil CNC 02",           "DEPT-WCA", "LINE-WC01", "CNC",              "Starfleet Engineering", "WC-CNC-MK7", "SN-WC-CNC-002", 45.0,  80.0,  "IOT-WC-CNC-02", "Connected"),
    ("MCH-WC-ASM01", "Dilithium Matrix Assembler 01","DEPT-WCA","LINE-WC02", "Assembly Robot",   "Daystrom Institute",   "DM-ASM-X9",  "SN-DM-ASM-001", 30.0,  120.0, "IOT-DM-ASM-01", "Connected"),
    ("MCH-HUL-WLD01","Hull Phaser Welder 01",       "DEPT-HUL", "LINE-HUL01","Plasma Welder",    "Starfleet Engineering", "PW-MK12",    "SN-PW-001",     60.0,   8.0,  "IOT-PW-01",     "Connected"),
    ("MCH-HUL-WLD02","Hull Phaser Welder 02",       "DEPT-HUL", "LINE-HUL02","Plasma Welder",    "Starfleet Engineering", "PW-MK12",    "SN-PW-002",     60.0,   8.0,  "IOT-PW-02",     "Connected"),
    ("MCH-SHU-ASM01","Shuttlecraft Assembly Station 01","DEPT-SBO","LINE-SHU01","Assembly Station","Utopia Manufacturing","SC-ASM-V3",  "SN-SC-001",     90.0,  30.0,  "IOT-SC-ASM-01", "Connected"),
    ("MCH-TAC-TRP01","Torpedo Casing Fabricator 01","DEPT-TAC", "LINE-TAC01","Fabricator",       "Starfleet Tactical",   "TC-FAB-MK5", "SN-TC-001",     20.0,  150.0, "IOT-TC-FAB-01", "Connected"),
    ("MCH-EPS-EXT01","EPS Conduit Extruder 01",     "DEPT-EPS", "LINE-EPS01","Extruder",         "Starfleet Engineering", "EPS-EXT-10", "SN-EPS-001",    15.0,  500.0, "IOT-EPS-EXT-01","Connected"),
    ("MCH-REP-ASM01","Replicator Line 01",          "DEPT-REP", "LINE-REP01","Replication Unit", "Daystrom Institute",   "REP-MK9",    "SN-REP-001",    10.0,  360.0, "IOT-REP-01",    "Connected"),
    ("MCH-REP-ASM02","Replicator Line 02",          "DEPT-REP", "LINE-REP01","Replication Unit", "Daystrom Institute",   "REP-MK9",    "SN-REP-002",    10.0,  360.0, "IOT-REP-02",    "Connected"),
    ("MCH-EPS-ASM05","EPS Assembly Station 05",     "DEPT-EPS", "LINE-EPS01","Assembly Station", "Starfleet Engineering", "EPS-ASM-5",  "SN-EPS-005",    25.0,  144.0, "IOT-EPS-ASM-05","Connected"),
    ("MCH-QA-TEST01","Federation Compliance Tester 01","DEPT-QA",null,       "Test Rig",         "Daystrom Institute",   "QA-TRIG-MK3","SN-QA-001",    120.0,  30.0,  "IOT-QA-01",     "Connected"),
};

var createdMachines = new Dictionary<string, Machine>();
foreach (var (code, name, deptCode, lineCode, type, mfr, model, serial, cycleTime, capHour, iotId, connState) in machineDefs)
{
    Guid? lineId = lineCode != null && createdLines.TryGetValue(lineCode, out var cl) ? cl.ProductionLineId : null;
    var m = await api.CreateMachineAsync(new Machine(
        Guid.Empty, tid, createdDepts[deptCode].DepartmentId, lineId,
        code, name, type, mfr, model, serial,
        InstallationDate: new DateOnly(2371, 3, 15),
        NominalCycleTimeSeconds: cycleTime,
        NominalCapacityPerHour: capHour,
        IoTDeviceId: iotId,
        IoTHubConnectionState: connState,
        CurrentStatus: "Running",
        LastStatusChangedAtUtc: DateTime.UtcNow.AddHours(-2),
        ExternalMicrosoft365Id: null, IsActive: true,
        CreatedAtUtc: DateTime.UtcNow, UpdatedAtUtc: DateTime.UtcNow));
    createdMachines[code] = m;
    Ok($"  Machine → {m.MachineCode,-20} {m.MachineName}");
}

// ══════════════════════════════════════════════════════════════════════════════
// 10. SHIFTS — Alpha / Beta / Gamma
// ══════════════════════════════════════════════════════════════════════════════
Section("10. Shifts — Alpha / Beta / Gamma");

var shiftDefs = new[]
{
    ("SHF-ALPHA", "Alpha Shift", new TimeOnly( 6, 0), new TimeOnly(14, 0), 30, false),
    ("SHF-BETA",  "Beta Shift",  new TimeOnly(14, 0), new TimeOnly(22, 0), 30, false),
    ("SHF-GAMMA", "Gamma Shift", new TimeOnly(22, 0), new TimeOnly( 6, 0), 30, true),
};

var createdShifts = new Dictionary<string, Shift>();
foreach (var (code, name, start, end, brk, night) in shiftDefs)
{
    var s = await api.CreateShiftAsync(new Shift(
        Guid.Empty, tid, code, name, start, end, brk, night,
        IsActive: true, CreatedAtUtc: DateTime.UtcNow));
    createdShifts[code] = s;
    Ok($"  Shift → {s.ShiftCode,-12} {s.ShiftName}  [{s.StartTime}–{s.EndTime}]");
}

// ══════════════════════════════════════════════════════════════════════════════
// 11. PRODUCTS — Starship components & assemblies
// ══════════════════════════════════════════════════════════════════════════════
Section("11. Products — Fleet Components Catalogue");

var productDefs = new[]
{
    ("PRD-WC-GX",  "Type-X Warp Core Assembly",           "EA",   3600.0, 480.0),
    ("PRD-WC-GXV", "Type-XV Quantum Warp Core Assembly",  "EA",   4800.0, 720.0),
    ("PRD-DIL-MTX","Dilithium Articulation Frame",        "EA",    900.0, 120.0),
    ("PRD-HUL-GAL","Galaxy-Class Hull Section",           "EA",  14400.0,1440.0),
    ("PRD-HUL-INT","Intrepid-Class Hull Section",         "EA",  10800.0, 960.0),
    ("PRD-HUL-DEF","Defiant-Class Ablative Hull Panel",   "EA",   2400.0, 300.0),
    ("PRD-SHU-TYP","Type-6 Shuttlecraft",                 "EA",   7200.0, 600.0),
    ("PRD-SHU-RUN","Runabout Danube-Class",               "EA",  18000.0,1200.0),
    ("PRD-TAC-TRP","Photon Torpedo Casing",               "EA",    180.0,  60.0),
    ("PRD-TAC-QTR","Quantum Torpedo Assembly",            "EA",    360.0,  90.0),
    ("PRD-TAC-PHA","Type-XII Phaser Array Module",        "EA",   1200.0, 180.0),
    ("PRD-EPS-CDT","EPS Conduit 10m",                     "MT",     15.0,  10.0),
    ("PRD-REP-MK9","Mark-IX Replicator Unit",             "EA",    600.0,  90.0),
    ("PRD-SHD-MDL","Regenerative Shield Emitter Module",  "EA",    900.0, 120.0),
};

var createdProducts = new Dictionary<string, Product>();
foreach (var (code, name, uom, cycle, setup) in productDefs)
{
    var p = await api.CreateProductAsync(new Product(
        Guid.Empty, tid, code, name, uom, cycle, setup,
        IsActive: true,
        CreatedAtUtc: DateTime.UtcNow, UpdatedAtUtc: DateTime.UtcNow));
    createdProducts[code] = p;
    Ok($"  Product → {p.ProductCode,-14} {p.ProductName}");
}

// ══════════════════════════════════════════════════════════════════════════════
// 12. WORK ORDERS — Fleet Orders from Starfleet Command
// ══════════════════════════════════════════════════════════════════════════════
Section("12. Work Orders — Fleet Orders");

var today = DateOnly.FromDateTime(DateTime.Today);

var workOrderDefs = new[]
{
    // (custCode, woNum, desc, status, priority, dueDate, plannedStart, totalQty)
    ("CUST-ENT",  "WO-NCC1701D-REFIT",   "NCC-1701-D Galaxy-Class Full Refit — Warp Core Replacement",        "InProgress",  1, today.AddDays(60),  today.AddDays(-10), 1.0),
    ("CUST-ENT",  "WO-NCC1701D-HULL",    "NCC-1701-D Hull Section Replacement — Decks 22-35",                 "InProgress",  1, today.AddDays(55),  today.AddDays(-8),  8.0),
    ("CUST-DSN",  "WO-DS9-SHLD-UPG",     "Deep Space Nine Shield Matrix Upgrade — Regenerative Array",        "Released",    2, today.AddDays(90),  today.AddDays(5),  12.0),
    ("CUST-DSN",  "WO-DEFIANT-UPG",      "Defiant-Class Shield Matrix & Ablative Armor Upgrade",              "Released",    2, today.AddDays(45),  today.AddDays(2),   4.0),
    ("CUST-VOY",  "WO-VOY-NACELLE",      "USS Voyager Nacelle Replacement — Intrepid-Class Type-XV",          "Draft",       3, today.AddDays(120), today.AddDays(30),  2.0),
    ("CUST-SFO",  "WO-ALPHA-SHU-FLEET",  "Starfleet Alpha Quadrant Shuttle Fleet Replenishment — 30 Units",   "Released",    2, today.AddDays(75),  today.AddDays(1),  30.0),
    ("CUST-MCK",  "WO-MCK-REPL-INST",    "McKinley Station Replicator Fleet Installation — Batch 2371-B",     "InProgress",  2, today.AddDays(30),  today.AddDays(-5), 20.0),
    ("CUST-KDF",  "WO-KDF-VORCHA-WC",    "Vor'Cha-Class Attack Cruiser Warp Core Upgrade — KDF Contract",     "Released",    3, today.AddDays(180), today.AddDays(45),  6.0),
    ("CUST-VUL",  "WO-VSA-SCIPROBE",     "Vulcan Science Academy Deep-Space Probe Drive Assembly",            "Draft",       4, today.AddDays(200), today.AddDays(60),  4.0),
    ("CUST-SFO",  "WO-EPS-CONDUIT-2371", "Starbase 375 EPS Conduit Replacement Programme 2371",               "InProgress",  2, today.AddDays(20),  today.AddDays(-15),500.0),
};

var createdWorkOrders = new Dictionary<string, WorkOrder>();
foreach (var (custCode, woNum, desc, status, pri, dueDate, plannedStart, qty) in workOrderDefs)
{
    var wo = await api.CreateWorkOrderAsync(new WorkOrder(
        Guid.Empty, tid, createdCustomers[custCode].CustomerId,
        woNum, desc, status, (byte)pri,
        DueDate: dueDate, PlannedStartDate: plannedStart,
        ActualStartDate: status == "InProgress" ? plannedStart : null,
        ActualEndDate: null,
        TotalQuantity: qty, CompletedQuantity: status == "InProgress" ? Math.Round(qty * 0.38, 1) : 0,
        RejectedQuantity: 0,
        ExternalMicrosoft365Id: null, TeamsChannelId: null,
        CreatedAtUtc: DateTime.UtcNow, UpdatedAtUtc: DateTime.UtcNow,
        CreatedByUserId: null, UpdatedByUserId: null));
    createdWorkOrders[woNum] = wo;
    Ok($"  WO → {wo.WorkOrderNumber,-26} [{wo.Status}]  Qty: {wo.TotalQuantity}");
}

// ══════════════════════════════════════════════════════════════════════════════
// 13. PRODUCTION ORDERS — Routed to machines
// ══════════════════════════════════════════════════════════════════════════════
Section("13. Production Orders — Floor Routing");

var prodOrderDefs = new[]
{
    // (woKey, prdCode, machCode, lineCode, seq, orderNum, status, planQty, goodQty, scrapQty, planStart, planEnd)
    ("WO-NCC1701D-REFIT", "PRD-WC-GXV",  "MCH-WC-CNC01",  "LINE-WC01", 10, "PO-2371-001", "InProgress", 1.0,  0.0,  0.0, -10, 20),
    ("WO-NCC1701D-REFIT", "PRD-DIL-MTX", "MCH-WC-ASM01",  "LINE-WC02", 20, "PO-2371-002", "InProgress", 2.0,  0.0,  0.0,  -8, 18),
    ("WO-NCC1701D-HULL",  "PRD-HUL-GAL", "MCH-HUL-WLD01", "LINE-HUL01",10, "PO-2371-003", "InProgress", 4.0,  2.0,  0.0,  -8, 15),
    ("WO-NCC1701D-HULL",  "PRD-HUL-GAL", "MCH-HUL-WLD02", "LINE-HUL01",20, "PO-2371-004", "InProgress", 4.0,  1.0,  0.0,  -5, 18),
    ("WO-DS9-SHLD-UPG",   "PRD-SHD-MDL", "MCH-WC-CNC02",  "LINE-WC01", 10, "PO-2371-005", "Released",   12.0, 0.0,  0.0,   5, 35),
    ("WO-DEFIANT-UPG",    "PRD-HUL-DEF", "MCH-HUL-WLD01", "LINE-HUL02",10, "PO-2371-006", "Released",    8.0, 0.0,  0.0,   2, 20),
    ("WO-DEFIANT-UPG",    "PRD-TAC-TRP", "MCH-TAC-TRP01", "LINE-TAC01",20, "PO-2371-007", "Released",   24.0, 0.0,  0.0,   3, 25),
    ("WO-ALPHA-SHU-FLEET","PRD-SHU-TYP", "MCH-SHU-ASM01", "LINE-SHU01",10, "PO-2371-008", "Released",   30.0, 0.0,  0.0,   1, 40),
    ("WO-MCK-REPL-INST",  "PRD-REP-MK9", "MCH-REP-ASM01", "LINE-REP01",10, "PO-2371-009", "InProgress",  10.0, 4.0,  0.0,  -5, 10),
    ("WO-MCK-REPL-INST",  "PRD-REP-MK9", "MCH-REP-ASM02", "LINE-REP01",20, "PO-2371-010", "InProgress",  10.0, 3.0,  1.0,  -4, 11),
    ("WO-EPS-CONDUIT-2371","PRD-EPS-CDT", "MCH-EPS-EXT01", "LINE-EPS01",10, "PO-2371-011", "InProgress", 300.0,148.0, 5.0, -15,  5),
    ("WO-EPS-CONDUIT-2371","PRD-EPS-CDT", "MCH-EPS-ASM05", "LINE-EPS01",20, "PO-2371-012", "InProgress", 200.0, 90.0, 3.0, -12,  8),
    ("WO-KDF-VORCHA-WC",  "PRD-WC-GX",   "MCH-WC-CNC01",  "LINE-WC01", 10, "PO-2371-013", "Released",    6.0,  0.0,  0.0,  45, 75),
    ("WO-VSA-SCIPROBE",   "PRD-WC-GX",   "MCH-WC-ASM01",  "LINE-WC02", 10, "PO-2371-014", "Planning",    4.0,  0.0,  0.0,  60, 90),
};

foreach (var (woKey, prdCode, machCode, lineCode, seq, orderNum, status, planQty, goodQty, scrapQty, startOff, endOff) in prodOrderDefs)
{
    var planStart = DateTime.UtcNow.AddDays(startOff);
    var planEnd   = DateTime.UtcNow.AddDays(endOff);
    var actualStart = status == "InProgress" ? planStart : (DateTime?)null;

    var po = await api.CreateProductionOrderAsync(new ProductionOrder(
        Guid.Empty, tid,
        createdWorkOrders[woKey].WorkOrderId,
        createdProducts[prdCode].ProductId,
        MachineId: createdMachines[machCode].MachineId,
        ProductionLineId: createdLines[lineCode].ProductionLineId,
        OperationSequence: seq,
        OrderNumber: orderNum,
        Status: status,
        PlannedQuantity: planQty,
        GoodQuantity: goodQty,
        ScrapQuantity: scrapQty,
        ReworkQuantity: 0,
        PlannedStartAtUtc: planStart,
        PlannedEndAtUtc: planEnd,
        ActualStartAtUtc: actualStart,
        ActualEndAtUtc: null,
        PlannedCycleTimeSeconds: null,
        ActualCycleTimeSeconds: null,
        CreatedAtUtc: DateTime.UtcNow,
        UpdatedAtUtc: DateTime.UtcNow));

    Ok($"  PO → {po.OrderNumber,-14} [{po.Status,-11}]  {planQty} × {prdCode}");
}

// ══════════════════════════════════════════════════════════════════════════════
// DONE
// ══════════════════════════════════════════════════════════════════════════════
Console.WriteLine();
Banner("SEEDING COMPLETE — Live Long and Prosper 🖖");
Console.WriteLine();

// ── Helpers ───────────────────────────────────────────────────────────────────
static void Banner(string text)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"╔══ {text} ══╗");
    Console.ResetColor();
}

static void Section(string text)
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"── {text}");
    Console.ResetColor();
}

static void Ok(string text)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("  ✓ ");
    Console.ResetColor();
    Console.WriteLine(text);
}

