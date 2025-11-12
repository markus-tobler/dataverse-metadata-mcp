# Dataverse MCP Server - Usage Examples for VS Code Agent Chat

This guide showcases how the Dataverse MCP Server empowers GitHub Copilot in Visual Studio Code to assist Power Platform developers with common development tasks. The MCP Server provides direct access to Dataverse metadata, enabling intelligent automation, validation, and code generation.

## Table of Contents

- [Getting Started](#getting-started)
- [Example 1: Create Data Model Documentation](#example-1-create-data-model-documentation)
- [Example 2: Compare Data Model to Implementation](#example-2-compare-data-model-to-implementation)
- [Example 3: Review Customizations Against Guidelines](#example-3-review-customizations-against-guidelines)
- [Example 4: Create Tables and Fields from Documentation](#example-4-create-tables-and-fields-from-documentation)

---

## Getting Started

To use the Dataverse MCP Server with GitHub Copilot in VS Code:

1. **Install and Configure** the Dataverse MCP Server from the extensions (see main README)
2. **Configure your connection string** in the MCP settings
3. **Start using** GitHub Copilot with `@workspace` commands
4. **Reference Dataverse** metadata directly in your conversations

Example queries to try:

- "What tables exist in our solution?"
- "Show me all choice fields in the Contact table"
- "Create documentation for the Account table"
- "What business rules are defined on the Opportunity table?"
- "Generate a plugin to validate email addresses on the Contact table"

---

## Example 1: Create Data Model Documentation

### Scenario

You're working on a Power Platform project and need to document the custom data model for the **Project Management** solution. You want to generate comprehensive documentation including tables, columns, relationships, and business rules.

### Conversation with GitHub Copilot

**You:**

> @workspace I need to create documentation for our Project Management solution in Dataverse. Can you generate a complete data model document for all custom tables starting with "new\_" prefix? Include columns, relationships, choice values, and business rules.

**GitHub Copilot (powered by Dataverse MCP Server):**

_[Copilot uses the MCP server to retrieve metadata]_

I'll generate comprehensive documentation for your Project Management solution. Let me retrieve the metadata...

_[Behind the scenes, Copilot calls:]_

- `ReadTables()` - Get all tables
- `ReadColumns(tableName)` - For each custom table
- `ReadEntityRelationships(tableName)` - Get relationships
- `ReadLocalOptionSets(tableName)` - Get choice values
- `ReadBusinessRules(tableName)` - Get business rules
- `ReadEntityForms(tableName)` - Get form information

**Result:**

Copilot creates a file `docs/DataModel-ProjectManagement.md`:

```markdown
# Project Management Data Model Documentation

## Overview

This document describes the custom data model for the Project Management solution.

## Tables

### 1. Project (new_project)

**Display Name:** Project  
**Plural Name:** Projects  
**Primary Column:** Project Name (new_name)  
**Owner:** User or Team owned  
**Features:** Auditing enabled, Business Process Flows enabled, Notes enabled

#### Columns

| Schema Name          | Display Name    | Type             | Required | Description              |
| -------------------- | --------------- | ---------------- | -------- | ------------------------ |
| new_name             | Project Name    | Text (100)       | Yes      | The name of the project  |
| new_startdate        | Start Date      | Date Only        | Yes      | Planned start date       |
| new_enddate          | End Date        | Date Only        | No       | Planned end date         |
| new_budget           | Budget          | Currency         | No       | Project budget           |
| new_status           | Status          | Choice           | Yes      | Current project status   |
| new_projectmanagerid | Project Manager | Lookup (Contact) | Yes      | Assigned project manager |

#### Status Choice Values

| Value | Label       | State    |
| ----- | ----------- | -------- |
| 1     | Planning    | Active   |
| 2     | In Progress | Active   |
| 3     | On Hold     | Active   |
| 4     | Completed   | Inactive |
| 5     | Cancelled   | Inactive |

#### Relationships

**One-to-Many:**

- `new_project_tasks`: Project → Tasks (1:N)
- `new_project_milestones`: Project → Milestones (1:N)

**Many-to-One:**

- `new_projectmanagerid`: Contact ← Project (N:1)

#### Business Rules

1. **Validate Dates**: End Date must be after Start Date
2. **Budget Warning**: Display warning when budget exceeds $1M
3. **Status Transition**: Prevent moving to Completed if active tasks exist

### 2. Task (new_task)

**Display Name:** Task  
**Plural Name:** Tasks  
**Primary Column:** Task Name (new_name)

[... continues for all tables ...]
```

**Value Delivered:**

- ✅ Complete documentation generated in minutes (vs. hours manually)
- ✅ Always accurate and up-to-date with current environment
- ✅ Includes all metadata: columns, relationships, choices, business rules
- ✅ Professional formatting ready for technical specifications

---

## Example 2: Compare Data Model to Implementation

### Scenario

Your team has a requirements document that specifies the needed data model, but you're not sure if the Dataverse environment matches the requirements. You want to validate that all required fields exist and identify any gaps.

### Conversation with GitHub Copilot

**You:**

> @workspace I have a requirements document in `docs/requirements.md` that lists all the fields we need for the Customer table. Can you compare this to our actual Dataverse implementation and tell me what's missing or different?

**Hint**: Try using this in combination with the Azure DevOPs MCP Server and get the requirements from your user stories.

**GitHub Copilot (powered by Dataverse MCP Server):**

Let me analyze your requirements against the current Dataverse implementation...

_[Copilot reads requirements.md and calls:]_

- `ReadTable("new_customer")`
- `ReadColumns("new_customer")`
- `ReadAttributeDetails("new_customer", columnName)` for each column

**Analysis Report:**

I've compared your requirements with the actual implementation. Here are the findings:

### ✅ Matching Fields (5)

These fields exist and match the requirements:

- `new_customername` (Text, 100) - Customer Name
- `new_email` (Email) - Email Address
- `new_phonenumber` (Phone) - Phone Number
- `new_accounttype` (Choice) - Account Type
- `new_createdon` (DateTime) - Created On

### ⚠️ Mismatches (2)

These fields exist but don't match requirements:

1. **new_creditlimit**

   - **Required:** Currency with max $500,000
   - **Actual:** Currency with max $100,000
   - **Action:** Update max value constraint

2. **new_isactive**
   - **Required:** Boolean with default "Yes"
   - **Actual:** Boolean with default "No"
   - **Action:** Change default value

### ❌ Missing Fields (3)

These required fields don't exist yet:

1. **new_industry** (Choice)

   - Type: Choice
   - Options: Technology, Healthcare, Finance, Retail, Other
   - Required: Yes

2. **new_annualrevenue** (Currency)

   - Type: Currency
   - Precision: 2
   - Required: No

3. **new_preferredcontactmethod** (Choice)
   - Type: Choice
   - Options: Email, Phone, SMS
   - Required: Yes

### 📋 Recommended Actions

1. Create the 3 missing fields
2. Update the creditlimit maximum value
3. Change the default value for isactive
4. Run solution checker to validate changes

Would you like me to:

- Generate the code to create the missing fields?
- Create a detailed implementation plan?
- Update the requirements document with actual implementation details?

**Value Delivered:**

- ✅ Instant validation of requirements vs. implementation
- ✅ Identifies gaps before they become production issues
- ✅ Provides actionable recommendations
- ✅ Saves hours of manual comparison work

---

## Example 3: Review Customizations Against Guidelines

### Scenario

Your organization has strict naming conventions and customization guidelines for Dataverse (following Microsoft best practices). You want to ensure all customizations in your solution comply with these standards before deploying to production.

### Conversation with GitHub Copilot

**You:**

> @workspace Review all customizations in the "ContosoSales" solution and check them against #file:Microsoft*Dataverse_naming_conventions.md and #file:best_practices.md. Our publisher prefix should be "contoso*". Generate a compliance report.

**GitHub Copilot (powered by Dataverse MCP Server):**

I'll analyze your ContosoSales solution for compliance with naming conventions and best practices...

_[Copilot calls:]_

- `ReadSolutionDetails("ContosoSales")`
- `ReadSolutionComponents("ContosoSales")`
- `ReadTables()` - Filter for solution components
- `ReadColumns(tableName)` - For each table
- `ReadEntityRelationships(tableName)`
- `ReadBusinessRules(tableName)`
- `ReadPluginSteps(tableName)`
- `ReadPublishers()` - Verify publisher prefix

**Compliance Report:**

### Dataverse Customization Compliance Report

**Solution:** ContosoSales  
**Publisher:** Contoso Corporation (contoso)  
**Generated:** 2024-11-12

---

### ✅ Naming Convention Compliance (85%)

#### Tables (8 total)

**Compliant (6):**

- ✅ `contoso_salesorder` - Proper prefix, singular noun, PascalCase
- ✅ `contoso_customer` - Proper prefix, singular noun
- ✅ `contoso_product` - Proper prefix, singular noun
- ✅ `contoso_invoice` - Proper prefix, singular noun
- ✅ `contoso_territory` - Proper prefix, singular noun
- ✅ `contoso_salesmetric` - Proper prefix, singular noun

**Non-Compliant (2):**

- ❌ `SalesQuotes` - **Missing publisher prefix** (should be `contoso_salesquote`)
- ❌ `contoso_order_items` - **Underscores in name** (should be `contoso_orderitem`)

#### Columns (147 total)

**Compliant (142):**

- ✅ Most columns follow proper naming: `contoso_customername`, `contoso_orderdate`, etc.

**Non-Compliant (5):**

1. ❌ Table: `contoso_salesorder`
   - Column: `Order_Number` - **Missing prefix and contains underscore** (should be `contoso_ordernumber`)
2. ❌ Table: `contoso_customer`
   - Column: `contoso_email_address` - **Underscore in name** (should be `contoso_emailaddress`)
3. ❌ Table: `contoso_product`
   - Column: `SKU` - **Missing prefix** (should be `contoso_sku`)
4. ❌ Table: `contoso_invoice`

   - Column: `contoso_DueDate` - **Incorrect casing** (should be `contoso_duedate`)

5. ❌ Table: `contoso_customer`
   - Column: `contoso_is_active` - **Underscore in name** (should be `contoso_isactive`)

#### Relationships (23 total)

**Compliant (20):**

- ✅ Most relationships follow `contoso_parenttable_N1_childtable` pattern

**Non-Compliant (3):**

- ❌ `salesorder_customer` - **Missing prefix** (should be `contoso_salesorder_N1_customer`)
- ❌ `contoso_product_orders` - **Poor naming** (should be `contoso_salesorder_N1_product`)
- ❌ `Territory_SalesRep` - **Missing prefix, wrong case** (should be `contoso_territory_N1_salesrep`)

---

### ⚠️ Best Practices Review

#### Plugin Performance

- ✅ All plugins use filtering attributes (good performance)
- ✅ No plugins on Retrieve/RetrieveMultiple (best practice)
- ⚠️ Warning: `contoso.SalesOrderPlugin` has 3 synchronous steps on Update - consider async
- ✅ ITracingService implemented in all plugins

#### Business Rules

- ✅ 12 business rules found - all properly configured
- ⚠️ Warning: Business rule "Validate Sales Amount" on `contoso_salesorder` is very complex (15 conditions)
  - Recommendation: Consider splitting into multiple rules or a plugin

#### Security

- ✅ Field-level security properly configured on sensitive columns
- ✅ All custom tables have appropriate security roles assigned
- ❌ Error: Security role "Sales Manager" has delete privileges on System Settings table
  - Fix: Remove delete privilege from system tables

#### Solution Structure

- ✅ All components are in a single solution (best practice)
- ✅ Solution uses managed properties correctly
- ⚠️ Warning: 3 unmanaged dependencies detected on standard tables
  - Recommendation: Document these dependencies

#### Choice Lists (Option Sets)

- ✅ Global choices used appropriately for shared values
- ⚠️ Table: `contoso_salesorder` has local choice with 45 options
  - Recommendation: Consider converting to global choice if used elsewhere

---

### 📊 Summary Statistics

| Category       | Compliant | Non-Compliant | Warnings |
| -------------- | --------- | ------------- | -------- |
| Tables         | 6 (75%)   | 2 (25%)       | 0        |
| Columns        | 142 (97%) | 5 (3%)        | 0        |
| Relationships  | 20 (87%)  | 3 (13%)       | 0        |
| Plugins        | 8 (100%)  | 0 (0%)        | 1        |
| Business Rules | 12 (100%) | 0 (0%)        | 1        |
| Security       | 95%       | 1 issue       | 0        |

**Overall Compliance Score: 85%**

---

### 🔧 Recommended Actions (Priority Order)

**High Priority (Before Production):**

1. Rename `SalesQuotes` table to `contoso_salesquote`
2. Remove delete privileges from Sales Manager role on system tables
3. Add publisher prefix to `Order_Number`, `SKU` columns
4. Fix relationship naming conventions

**Medium Priority:** 5. Remove underscores from column names 6. Consider splitting complex business rule 7. Evaluate async processing for SalesOrderPlugin

**Low Priority:** 8. Document unmanaged dependencies 9. Consider global choice conversion for large local options

Would you like me to:

- Generate a detailed remediation plan with step-by-step instructions?
- Create a script to rename the non-compliant components?
- Export the findings to a Word document for stakeholder review?

**Value Delivered:**

- ✅ Comprehensive compliance check in minutes
- ✅ Identifies issues before code review or deployment
- ✅ Aligned with Microsoft best practices and naming conventions
- ✅ Prioritized recommendations for remediation
- ✅ Reduces technical debt and improves maintainability

---

## Example 4: Create Tables and Fields from Documentation

### Scenario

Your business analyst has created a detailed requirements document in Markdown describing the needed data model for a new **Equipment Tracking** feature. You want to use this documentation to automatically generate the Dataverse tables and fields, ensuring consistency between documentation and implementation.

### Conversation with GitHub Copilot

**You:**

> @workspace I have a requirements document at `docs/equipment-tracking-requirements.md` that describes the data model we need. Can you read this document and create the corresponding tables and fields in our Dataverse environment? Use the prefix "eq\_" for all components.

**Context - The Requirements Document:**

```markdown
# Equipment Tracking System Requirements

## Tables

### Equipment

- **Display Name:** Equipment
- **Plural Name:** Equipment Items
- **Description:** Tracks physical equipment assets

#### Fields:

1. **Equipment Name** (Text, 100 characters, Required)
   - The name/model of the equipment
2. **Serial Number** (Text, 50 characters, Required, Unique)
   - Manufacturer serial number
3. **Category** (Choice, Required)
   - Options: Computer, Phone, Tablet, Printer, Other
4. **Status** (Choice, Required, Default: Available)
   - Options: Available, In Use, Under Repair, Retired
5. **Purchase Date** (Date Only, Required)
   - Date equipment was purchased
6. **Purchase Cost** (Currency, Required)
   - Original purchase cost
7. **Assigned To** (Lookup to Contact, Optional)
   - Currently assigned employee
8. **Location** (Text, 200 characters, Optional)
   - Current physical location

### Equipment Assignment

- **Display Name:** Equipment Assignment
- **Plural Name:** Equipment Assignments
- **Description:** Tracks equipment assignment history

#### Fields:

1. **Assignment Name** (Auto-number: "ASSIGN-{SEQNUM:5}")
2. **Equipment** (Lookup to Equipment, Required)
3. **Assigned To** (Lookup to Contact, Required)
4. **Assignment Date** (Date and Time, Required)
5. **Return Date** (Date and Time, Optional)
6. **Notes** (Multiline Text, 500 characters, Optional)
   - Assignment notes or conditions
```

**GitHub Copilot (powered by Dataverse MCP Server):**

I'll analyze your requirements document and create the Dataverse tables and fields. Let me start by reading the requirements...

_[Copilot reads the markdown file and plans the implementation]_

I've analyzed your requirements. Here's my implementation plan:

**Tables to Create:**

1. Equipment (eq_equipment)
2. Equipment Assignment (eq_equipmentassignment)

**Fields to Create:**

- Equipment: 8 fields (including 2 choice fields and 1 lookup)
- Equipment Assignment: 6 fields (including 2 lookups and 1 multiline text)

**Relationships:**

- Equipment → Equipment Assignment (1:N)
- Contact → Equipment (N:1) - for "Assigned To"
- Contact → Equipment Assignment (N:1) - for "Assigned To"

Let me create these components...

_[Copilot executes the following MCP calls:]_

```
1. CreateTable("eq_equipment", "Equipment", "Equipment Items", ...)
2. CreateTextColumn("eq_equipment", "eq_equipmentname", ...)
3. CreateTextColumn("eq_equipment", "eq_serialnumber", ...)
4. CreateChoiceColumn("eq_equipment", "eq_category", ..., "Computer:1,Phone:2,Tablet:3,Printer:4,Other:5")
5. CreateChoiceColumn("eq_equipment", "eq_status", ..., "Available:1,In Use:2,Under Repair:3,Retired:4")
6. CreateDateTimeColumn("eq_equipment", "eq_purchasedate", ...)
7. CreateCurrencyColumn("eq_equipment", "eq_purchasecost", ...)
8. CreateLookupColumn("eq_equipment", "eq_assignedto", ..., targetTable: "contact")
9. CreateTextColumn("eq_equipment", "eq_location", ...)

10. CreateTable("eq_equipmentassignment", "Equipment Assignment", "Equipment Assignments", ...)
11. CreateTextColumn("eq_equipmentassignment", "eq_assignmentname", ...) // Will be auto-number
12. CreateLookupColumn("eq_equipmentassignment", "eq_equipmentid", ..., targetTable: "eq_equipment")
13. CreateLookupColumn("eq_equipmentassignment", "eq_assignedto", ..., targetTable: "contact")
14. CreateDateTimeColumn("eq_equipmentassignment", "eq_assignmentdate", ...)
15. CreateDateTimeColumn("eq_equipmentassignment", "eq_returndate", ...)
16. CreateMultilineTextColumn("eq_equipmentassignment", "eq_notes", maxLength: 500)
```

**Result:**

✅ **Successfully Created:**

**Tables:**

1. ✅ Equipment (eq_equipment)
   - Primary field: Equipment Name (eq_name)
   - 8 custom fields created
2. ✅ Equipment Assignment (eq_equipmentassignment)
   - Primary field: Assignment Name (eq_name)
   - 6 custom fields created

**Relationships:**

1. ✅ eq_equipment_N1_contact (Equipment → Contact for "Assigned To")
2. ✅ eq_equipmentassignment_N1_equipment (Equipment Assignment → Equipment)
3. ✅ eq_equipmentassignment_N1_contact (Equipment Assignment → Contact)

**Summary Report:**

```markdown
# Equipment Tracking System - Implementation Complete

## Created Components

### Table: Equipment (eq_equipment)

| Field          | Schema Name      | Type             | Properties                                |
| -------------- | ---------------- | ---------------- | ----------------------------------------- |
| Equipment Name | eq_equipmentname | Text             | Max: 100, Required ✓                      |
| Serial Number  | eq_serialnumber  | Text             | Max: 50, Required ✓                       |
| Category       | eq_category      | Choice           | 5 options, Required ✓                     |
| Status         | eq_status        | Choice           | 4 options, Required ✓, Default: Available |
| Purchase Date  | eq_purchasedate  | Date Only        | Required ✓                                |
| Purchase Cost  | eq_purchasecost  | Currency         | Required ✓, Precision: 2                  |
| Assigned To    | eq_assignedto    | Lookup (Contact) | Optional                                  |
| Location       | eq_location      | Text             | Max: 200, Optional                        |

### Table: Equipment Assignment (eq_equipmentassignment)

| Field           | Schema Name       | Type               | Properties         |
| --------------- | ----------------- | ------------------ | ------------------ |
| Assignment Name | eq_assignmentname | Text               | Auto-number format |
| Equipment       | eq_equipmentid    | Lookup (Equipment) | Required ✓         |
| Assigned To     | eq_assignedto     | Lookup (Contact)   | Required ✓         |
| Assignment Date | eq_assignmentdate | DateTime           | Required ✓         |
| Return Date     | eq_returndate     | DateTime           | Optional           |
| Notes           | eq_notes          | Multiline Text     | Max: 500, Optional |

## Next Steps

I recommend:

1. ✅ Create forms for data entry (Main form, Quick create form)
2. ✅ Create views (Active Equipment, Available Equipment, Assignment History)
3. ✅ Configure business rules (e.g., prevent assigning equipment already in use)
4. ✅ Set up security roles for equipment managers
5. ✅ Consider creating a model-driven app for equipment management

Would you like me to:

- Generate the business rules described in your requirements?
- Create documentation for the implemented data model?
- Set up basic forms and views?
- Create a Power Apps canvas app for equipment check-out?
```

**Value Delivered:**

- ✅ Rapid implementation from documentation (minutes vs. hours)
- ✅ Consistency between requirements and implementation
- ✅ No manual data entry errors
- ✅ Proper naming conventions enforced automatically
- ✅ Relationships created automatically
- ✅ Intelligent recommendations for business logic implementation
- ✅ Reduces development time

---

## Key Benefits Summary

### For Developers

- **Instant Metadata Access**: No need to manually navigate Dataverse UI or write queries
- **Best Practices Enforcement**: Built-in validation against Microsoft guidelines
- **Documentation Automation**: Keep documentation in sync with implementation

### For Teams

- **Faster Onboarding**: New developers can query the data model conversationally
- **Quality Assurance**: Automated compliance checking before deployment
- **Reduced Errors**: Generate components from documentation to ensure consistency
- **Knowledge Sharing**: Documentation becomes actionable code

### For Organizations

- **Reduced Development Time**: 50-80% faster for common tasks
- **Lower Technical Debt**: Consistent naming and patterns across solutions
- **Better Compliance**: Automated checks against organizational standards
- **Improved Quality**: Fewer manual errors in metadata creation

---

## Best Practices

### When Using the MCP Server

1. **Be Specific**: Reference exact table names and schema names for best results
2. **Use Publisher Prefixes**: Always specify the correct publisher prefix for your organization
3. **Review Generated Code**: While the MCP server provides accurate metadata, always review generated code before deployment
4. **Combine with Solution Checker**: Use the Power Platform solution checker for additional validation
5. **Version Control**: Keep your documentation in source control alongside your code

### Development Workflow

```
1. Document Requirements (Markdown) →
2. Use Copilot to Compare vs. Implementation →
3. Use Copilot to Generate Missing Components →
4. Use Copilot to Validate Against Guidelines →
5. Deploy and Document
```

---

## Learn More

- [Dataverse Best Practices](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/best-practices/)
- [Naming Conventions](https://learn.microsoft.com/en-us/microsoft-365/community/cds-and-model-driven-apps-standards-and-naming-conventions)
- [Plugin Development](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/plug-ins)
- [Solution Lifecycle Management](https://learn.microsoft.com/en-us/power-platform/alm/)
