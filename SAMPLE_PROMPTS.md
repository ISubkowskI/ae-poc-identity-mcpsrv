# Sample Prompts for AE Identity Claims MCP Server

This content describes how to interact with the `ae-identity-claims` MCP server using natural language prompts.

## 1. System Information

**Prompt:**
> "What version of the identity claims server is currently running?"

*Tool Used:* `general-get_app_version`
*Description:* Retrieves the application name, version, and server time.

## 2. Listing Claims

**Prompt:**
> "List the first 20 identity claims."

*Tool Used:* `identity-get_claims`
*Arguments:* 
```json
{
  "queryIncomingDto": {
    "skipped": 0,
    "numberOf": 20
  }
}
```

**Prompt:**
> "Fetch all claims and show me their details."
> "Fetch all claims and show me their DisplayText and IDs."

*Tool Used:* `identity-get_claims`
*Arguments:* 
```json
{
  "queryIncomingDto": {
    "skipped": 0,
    "numberOf": 100 // Default batch size
  }
}
```

## 3. Retrieving Claim Details

**Prompt:**
> "Get details for the claim with ID `f03af1bf-17d4-455d-9237-bf53fdd2eab6`."

*Tool Used:* `identity-get_claim_details`
*Arguments:* `claimId`

## 4. Creating a Claim

**Prompt:**
> "Create a new role claim. The type should be `role`, value `Administrator2`, value type `string`, and display text `Admin Role2`, description `Administrator2 role with full permissions`."

*Tool Used:* `identity-create_claim`
*Arguments:*
```json
{
  "claimDto": {
    "Type": "role",
    "Value": "Administrator2",
    "ValueType": "string",
    "DisplayText": "Admin Role2",
    "Description": "Administrator2 role with full permissions"
  }
}
```

## 5. Updating a Claim

**Prompt:**
> "Update the claim `486a3725-...` to have the display text 'Super Admin'."

*Tool Used:* `identity-update_claim`
*Arguments:* `claimId`, `claimDto` (typically requires fetching the claim first to get the current state, or providing the full object).

## 6. Deleting a Claim

**Prompt:**
> "Delete the claim with ID `486a3725-...`."

*Tool Used:* `identity-delete_claim`
*Arguments:* `claimId`
