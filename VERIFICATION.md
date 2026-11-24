# HubLink — Acceptance & Verification Checklist

- **Service Endpoint Tested:** `http://127.0.0.1:5080` (HubLink Local Service)
- **Sample CSV Deliverable:** `sample-export-BR-001.csv`
- **Incomplete Scenarios:** None (8 of 8 verified)

| Scenario ID | Acceptance Scenario | Status | How It Was Checked |
| :--- | :--- | :--- | :--- |
| **AC-01** | Retrieve shipment information from the provided API for a configured branch | Verified | Started `Start-HubLink.cmd` and retrieved `BR-001` (5 records), `BR-002` (2 records), and `BR-003` (1 record) via `GET /api/shipments`; confirmed real HTTP retrieval and accurate display of Tracking Number, Origin, Destination, Status, and Last Updated (UTC). |
| **AC-02** | Apply different shipment status filters | Verified | Loaded `BR-001` (5 records) and switched the status filter across `Pending` (1), `InTransit` (2), `Delivered` (1), and `Exception` (1) locally without additional HTTP requests; selected `All` to restore all 5 records. |
| **AC-03** | Examine the operational summary | Verified | Confirmed the summary cards displayed `Total: 5`, `Pending: 1`, `InTransit: 2`, `Delivered: 1`, and `Exception: 1` for `BR-001`, accurately reflecting all retrieved branch shipments regardless of the active table filter. |
| **AC-04** | Refresh information from the service | Verified | Clicked **Refresh** on `/api/shipments` and `/api/test/shipments?branchCode=BR-001&mode=delay` (with a 15-second timeout); confirmed the loading bar appeared, **Refresh** disabled to prevent duplicate requests, and the window remained responsive. |
| **AC-05** | Encounter an unavailable service, request timeout, or unsuccessful HTTP response | Verified | Tested a stopped local server (connection refused), `mode=delay` with a 5-second timeout, `mode=bad-request` (HTTP 400), and `mode=server-error` (HTTP 500); confirmed clear error messages, stale-data banner when prior data was loaded, and recovery via **Retry**. |
| **AC-06** | Receive an empty or invalid response | Verified | Tested `branchCode=BR-999` and `mode=empty` (handled cleanly as a valid 0-record empty result) versus `mode=invalid-json` (caught malformed JSON and displayed an error alert without crashing). |
| **AC-07** | Export shipment information while a status filter is active | Verified | Exported `BR-001` under the `InTransit` filter (2 rows) and `All` filter (5 rows into `sample-export-BR-001.csv`); verified exact header order (`Tracking Number,Origin,Destination,Status,LastUpdatedUtc`) and RFC 4180 escaping of commas and quotation marks. |
| **AC-08** | Attempt an export that cannot be completed | Verified | Tested clicking **Export CSV** when 0 records were displayed and when the target CSV file was locked by another application; confirmed clear error feedback without falsely claiming a file was created. |