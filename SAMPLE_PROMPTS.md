# Sample Prompts for AE Identity Claims MCP Server

This content describes how to interact with the `ae-identity-claims` MCP server using natural language prompts.

## 1. System Information

**Prompt:**
```text
Do you know 'ae-identity-claims' MCP server?
```
```text
Sind Sie mit dem MCP-Server „ae-identity-claims“ vertraut? (Antwort auf Deutsch)
```
```text
What version of the identity claims server is currently running?
```

*Tool Used:* `general-get_app_version`
*Description:* Retrieves the application name, version, and server time.

## 2. Listing Claims

**Prompt:**
```text
List the first 10 identity claims.
```

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
```text
Fetch all claims and show me their details.
```

```text
Fetch all claims and show me their DisplayText and IDs. Format as table. Add sequence number as first column.
```

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
```text
Get details for the claim with ID `5a9f467e-24a3-48e5-a4fc-46edefbcfdd4`.
```

*Tool Used:* `identity-get_claim_details`
*Arguments:* `claimId`

## 4. Creating a Claim

**Prompt:**
```text
Create a new role claim. The type should be `role`, value `Administrator2`, value type `string`, and display text `Admin Role2`, description `Administrator2 role with full permissions`.
```

```text
Get details for the claim with DisplayText `Admin Role2`.
```

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
```text
Update the claim `8d03f9ba-590c-4b71-9e99-3754f0e7cb47` to have the display text 'Super Admin Role2'.
```

```text
Update the claim `Super Admin Role2` to have the display text 'Super Admin Role200'.
```

*Tool Used:* `identity-update_claim`
*Arguments:* `claimId`, `claimDto` (typically requires fetching the claim first to get the current state, or providing the full object).

## 6. Deleting a Claim

**Prompt:**
```text
Delete the claim with ID `893f172e-250c-4c19-a0c9-376445f65670`.
```

*Tool Used:* `identity-delete_claim`
*Arguments:* `claimId`
