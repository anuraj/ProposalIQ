## 1. Controller Error Handling & View State Preservation

- [x] 1.1 Update `HomeController.Analyze` to populate `ViewBag.ModelStatus` in all validation/error return paths
- [x] 1.2 Ensure `HomeController.Analyze` handles `OperationCanceledException` by adding a model error and returning `View("Index", request)` instead of raw `BadRequest`

## 2. Frontend Error Banner & Form Presentation

- [x] 2.1 Add server-rendered alert banner for `ModelState` validation errors and analysis failures in `Index.cshtml`
- [x] 2.2 Retain user-entered deal context input values in `Index.cshtml` form fields when re-rendered on failure
- [x] 2.3 Update client-side script in `Index.cshtml` to hide or clear server error banners when a new proposal file is selected

## 3. Testing & Verification

- [x] 3.1 Update and add unit tests in `Tests/HomeControllerTests.cs` verifying `ViewBag.ModelStatus` population and `ModelState` error messages on analysis failures
- [x] 3.2 Add tests verifying cancellation returns the index view with model errors rather than unhandled response
- [x] 3.3 Run `dotnet build` and `dotnet test` to verify clean build and all test suites passing
