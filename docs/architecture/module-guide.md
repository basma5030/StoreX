# StoreX Module Architecture Guide

## 1. Architecture Overview

StoreX follows a Modular Monolith architecture.

Each module owns its domain logic, application services, endpoints, configurations, and internal data structures.

Current modules:
- Organization
- Identity
- Catalog
- Accounting
- Parties
- Treasury

## 2. Module Folder Responsibilities

Each module follows this general structure:

- Application: Application services and use cases.
- Configurations: Entity Framework Core entity configurations.
- Contracts: Public interfaces and DTOs exposed to other modules.
- Domain: Domain entities, rules, and value objects.
- Endpoints: Module API endpoints.
- Seeding: Module-specific seed data.

The Organization module currently also has an Entities folder. Follow the established project conventions when working in existing modules.

## 3. Module Ownership

Each contributor owns the implementation of their assigned module.

Module owners may create and modify files inside their own module.

Changes to shared infrastructure or another module must be coordinated with the responsible owner before implementation.

## 4. Communication Between Modules

- Modules must not access another module's internal classes or implementation details.
- Cross-module communication must use public Contracts.
- A module must not directly query or modify another module's database tables.
- Required lookups and operations should be exposed through interfaces in the owning module's Contracts folder.
- Avoid circular dependencies between modules.

## 5. Entity Framework Core Rules

- Each module owns its entity classes and IEntityTypeConfiguration<T> configurations.
- Entity configurations belong in that module's Configurations folder.
- Module tables should use their respective database schemas.
- Current shared DbContext configuration discovers entity configurations through ApplyConfigurationsFromAssembly.
- Module owners should document the entities, relationships, indexes, and constraints they introduce.

Suggested schemas:
- Organization: organization
- Identity: identity
- Catalog: catalog
- Accounting: accounting
- Parties: parties
- Treasury: treasury
- Auditing: audit

## 6. DbContext and DbSets

StoreXDbContext is shared infrastructure.

- Module owners must coordinate required DbSet additions with Person 1.
- Do not independently modify shared DbContext files without coordination.
- Include DbContext changes in the relevant Pull Request when required.
- Describe all required database changes in the PR.

## 7. Migrations Ownership

Person 1 is responsible for generating and maintaining central EF Core migrations and the model snapshot.

- Contributors must not independently generate migrations.
- Do not create parallel migrations from different module branches.
- After reviewing and integrating module changes, Person 1 updates the central migration.
- Database changes must be reviewed before the migration is generated.

## 8. Pull Request Rules

- All contributions must be submitted through Pull Requests.
- Contributors must work on separate feature branches.
- Do not push directly to main.
- PRs must describe the changes and include relevant test results.
- Shared infrastructure changes must be coordinated with Person 1.
- Person 1 reviews and tests changes before merging.

## 9. Accounting, Parties, and Treasury Coordination

- Accounting and Parties can begin their initial implementation in parallel.
- Treasury implementation should begin after the required Accounting and Parties contracts and dependencies are agreed upon and stable.
- Accounting and Treasury owners should coordinate posting and integration requirements.
- The initial Treasury scope does not introduce a separate PaymentMethod entity.
- Receipts and payments initially post directly to the relevant cashbox or bank account.

## 10. Testing Expectations

- Add tests for new business rules and application behavior.
- Add integration tests for database-dependent behavior where appropriate.
- Verify module boundaries and avoid direct cross-module database access.
- Run the relevant tests before opening a Pull Request.
