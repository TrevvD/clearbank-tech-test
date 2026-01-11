### Identified Improvements
- SOLID
   - Single Responsibility - PaymentService clearly doing way too much, it's deciding validation for various schemes, updating account values, persisting the results. 
   - Open/Closed - Adding a new payment method requires modifying PaymentService, as per-service logic is included in this file.
   - Interface Segregation - DataStore objects can share an interface, as will payment method validators.
   - Dependency Inversion - Objects being directly instantiated within the service mean they can't be tested, should be injected using DI.
- Testability
  - Return for MakePayment only contains a true/false, rather than any information about why, means tests can't guarantee they fail for exactly one reason.
  - Lack of interfaces means mocking is impossible.
  - Direct instantiation of objects, rather than injection, means we can't inject mocks or test doubles.
- Readability
  - Very difficult to read, due to levels of nesting, high cognitive complexity.
  - Large chunks of code irrelevant depending on the type of payment you're making.
  - Shared logic per payment type repeated.

## Further Improvements
- Persist the account values using an in-memory DB
- Add CRUD operations for Accounts.
- Spin up the actual API using WebApplicationFactory.
- Add various integration tests, removing the dependency on some of the brittle unit tests.
- Add Swagger/OpenAPI generation.
- Containerise the API for portability.