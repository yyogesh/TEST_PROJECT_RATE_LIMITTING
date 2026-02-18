# Kentico Xperience Admin Panel - Step-by-Step Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Accessing the Admin Panel](#accessing-the-admin-panel)
3. [Step 1: Create Headless Channel](#step-1-create-headless-channel)
4. [Step 2: Create Content Type](#step-2-create-content-type)
5. [Step 3: Add Fields to Content Type](#step-3-add-fields-to-content-type)
6. [Step 4: Assign Content Type to Channel](#step-4-assign-content-type-to-channel)
7. [Step 5: Generate API Key](#step-5-generate-api-key)
8. [Step 6: Create Headless Content Items](#step-6-create-headless-content-items)
9. [Step 7: Publish Content Items](#step-7-publish-content-items)
10. [Verification Checklist](#verification-checklist)
11. [Common Admin Panel Issues](#common-admin-panel-issues)

---

## Introduction

This document provides a detailed, step-by-step guide for configuring Kentico Xperience Headless CMS through the admin panel. Each step includes:
- Exact navigation paths
- What to click
- What values to enter
- What to expect after each action
- Why each step is necessary

**Prerequisites:**
- Kentico Xperience application is running
- You have administrator access
- Database is configured and connected

---

## Accessing the Admin Panel

### Step-by-Step Instructions

1. **Start the Kentico Application**
   - Run your ASP.NET Core application
   - Wait for the application to fully initialize
   - Note the port number (e.g., `http://localhost:43907`)

2. **Navigate to Admin Panel**
   - Open your web browser
   - Go to: `http://localhost:[port]/admin`
   - Example: `http://localhost:43907/admin`

3. **Login**
   - Enter your administrator username
   - Enter your administrator password
   - Click **"Sign in"** or press Enter

4. **Verify Access**
   - You should see the Kentico admin dashboard
   - Left sidebar should show navigation menu
   - Top bar should show your user information

**What You'll See:**
- Main navigation menu on the left
- Content tree or dashboard in the center
- Toolbar at the top

**Important:** If you cannot access the admin panel, verify:
- Application is running
- Database connection is working
- You have administrator privileges

---

## Step 1: Create Headless Channel

### Purpose
Channels organize your content and provide unique GraphQL API endpoints. Each channel has its own endpoint URL that clients use to access content.

### Why This Step is Needed
- Without a channel, there is no API endpoint
- Channels separate content for different applications (e.g., Angular app, mobile app)
- Each channel generates a unique identifier used in the GraphQL URL

### Detailed Steps

#### 1.1 Navigate to Channel Management

1. In the left sidebar, locate **"Configuration"**
2. Click on **"Configuration"** to expand it
3. Look for **"Channel management"** under Configuration
4. Click on **"Channel management"**

**What You'll See:**
- A list of existing channels (if any)
- A button or link to create a new channel
- Channel management interface

#### 1.2 Create New Channel

1. Look for the **"New channel"** button (usually at the top of the page)
2. Click **"New channel"**

**What You'll See:**
- A dialog or page with channel type options
- Options may include:
  - Website channel
  - Headless channel
  - Other channel types

#### 1.3 Select Headless Channel Type

1. From the channel type options, select **"Headless channel"**
2. The channel type should be automatically set to "Headless"

**Note:** If "Headless channel" is not visible, ensure you have the correct Kentico Xperience version with headless support.

#### 1.4 Configure Channel Settings

You'll see a form with several tabs. Focus on the **"General"** tab first.

**Fill in the following fields:**

1. **Channel name:**
   - Enter: `Angular App` (or your preferred name)
   - This is a display name for your reference
   - Example: "Angular App", "Mobile App", "API Channel"

2. **Channel type:**
   - Should be automatically set to **"Headless"**
   - If not, select "Headless" from the dropdown

3. **Channel size:**
   - Select: **"Standard channel"**
   - This determines the capacity and features available

4. **Primary language:**
   - Select: **"English"** (or your default language)
   - This sets the default language for content

**Additional Settings (if visible):**
- **Site name:** May auto-populate from channel name
- **Domain:** Leave default or configure as needed
- **Culture:** Should match primary language

#### 1.5 Save the Channel

1. Review all entered information
2. Click the **"SAVE"** button (usually at the bottom of the form)
3. Wait for the save confirmation

**What Happens:**
- Channel is created in the database
- A unique channel identifier (GUID) is generated
- GraphQL endpoint URL is created
- You are redirected to the channel details page

#### 1.6 Note the Channel Identifier

After saving, you'll see the channel details page with several tabs.

1. Make sure you're on the **"General"** tab
2. Look for a section labeled **"GraphQL API endpoint"** or **"API Endpoint"**
3. You'll see a URL like:
   ```
   http://localhost:43907/graphql/ca81ab98-d0fa-44b4-b6e8-4b38b34cb8f9
   ```
4. **IMPORTANT:** Copy the GUID part (the long string after `/graphql/`)
   - Example: `ca81ab98-d0fa-44b4-b6e8-4b38b34cb8f9`
   - This is your **Channel Identifier**

**Where to Find It:**
- Channel management → [Your Channel Name] → General tab
- Look for "GraphQL API endpoint" section
- The GUID is the last part of the URL

**Status Indicator:**
- You may see "Status: Inactive" or "Endpoint: Inactive"
- This is normal - the endpoint becomes active after you assign a content type

**Save This Information:**
- Channel Name: `Angular App`
- Channel Identifier: `[your-guid-here]`
- Full GraphQL URL: `http://localhost:43907/graphql/[your-guid-here]`

---

## Step 2: Create Content Type

### Purpose
Content types define the structure of your content. They specify what fields (data) your content items will have. Think of it as a template or schema.

### Why This Step is Needed
- Without a content type, you cannot create content items
- Content types define what data can be stored
- Each content type becomes a GraphQL type in the API

### Detailed Steps

#### 2.1 Navigate to Content Types

1. In the left sidebar, locate **"Configuration"**
2. Click on **"Configuration"** to expand it
3. Look for **"Content types"** under Configuration
4. Click on **"Content types"**

**What You'll See:**
- A list of existing content types (if any)
- A button to create a new content type
- Content type management interface

#### 2.2 Create New Content Type

1. Look for the **"NEW CONTENT TYPE"** button (usually at the top)
2. Click **"NEW CONTENT TYPE"**

**What You'll See:**
- A form with multiple tabs:
  - General
  - Fields
  - Relationships
  - Other tabs depending on configuration

#### 2.3 Configure General Tab

Stay on the **"General"** tab (should be selected by default).

**Fill in the following fields:**

1. **Content type name:**
   - Enter: `Article`
   - This is the display name
   - Example: "Article", "Blog Post", "Product"

2. **Code name - Namespace:**
   - Enter: `Article`
   - This groups related content types
   - Example: "Article", "Blog", "Ecommerce"
   - **Important:** Use PascalCase (first letter uppercase)

3. **Code name - Name:**
   - Enter: `Article`
   - This is the specific content type identifier
   - Example: "Article", "Post", "Product"
   - **Important:** Use PascalCase (first letter uppercase)

**Resulting Code Name:**
- The full code name will be: `Article.Article` (namespace.name format)
- This is what appears in the system

4. **Use for:**
   - **CRITICAL:** Check the box for **"Headless"**
   - This enables the content type for headless channels
   - Without this, the content type won't be available in headless channels
   - You may see other options like "Pages", "Email campaigns" - you can check those too if needed, but "Headless" is required

**Additional Settings (if visible):**
- **Display name:** May auto-populate
- **Description:** Optional - add a description if desired
- **Icon:** Optional - select an icon for the admin interface

#### 2.4 Save the Content Type

1. Review all entered information
2. Ensure "Headless" is checked under "Use for"
3. Click the **"SAVE"** button (usually at the bottom)
4. Wait for the save confirmation

**What Happens:**
- Content type is created
- You are redirected to the content type details page
- The content type now appears in the list
- GraphQL type name will be: `articleArticle` (camelCase of namespace + name)

**Important Notes:**
- The GraphQL field name is automatically generated as: `articleArticle`
- This is the camelCase version of `Article.Article`
- You'll use this name in GraphQL queries

**What's Next:**
- The content type is created but has no fields yet
- You need to add fields (Step 3) before it's useful
- The content type must be assigned to a channel (Step 4) before it's accessible via API

---

## Step 3: Add Fields to Content Type

### Purpose
Fields define the actual data structure of your content. Each field represents a piece of information you want to store (like title, body, author, date, etc.).

### Why This Step is Needed
- Without fields, content types are empty templates
- Fields become GraphQL fields in the API
- Fields determine what data you can query and retrieve

### Detailed Steps

#### 3.1 Navigate to Content Type Fields

1. You should still be on the content type details page after creating it
2. If not, go to: **Configuration → Content types → Article**
3. Click on the **"Fields"** tab

**What You'll See:**
- A list of existing fields (initially empty)
- A button to add new fields
- Field management interface

#### 3.2 Add Title Field

1. Click the **"NEW FIELD"** button (usually at the top of the fields list)

**Field Configuration Form:**

1. **Field name:**
   - Enter: `Title`
   - This is the display name shown in the admin interface
   - Example: "Title", "Headline", "Name"

2. **Data type:**
   - Select: **"Text"** from the dropdown
   - This creates a single-line text field
   - Other options: "Long text", "Number", "Date/Time", etc.

3. **Required:**
   - Check the box: **"Required"** or set to **"Yes"**
   - This ensures every content item must have a title
   - Prevents creating incomplete content

4. **Field code name:**
   - May auto-generate as: `Title` or `title`
   - This is used in GraphQL queries
   - Usually lowercase version of field name
   - **Important:** Note this - you'll use it in queries (likely `title`)

**Additional Settings (if visible):**
- **Default value:** Leave empty (optional)
- **Help text:** Optional - add description if desired
- **Validation:** Optional - add rules if needed

5. Click **"SAVE"** to save the field

**What Happens:**
- Field is added to the content type
- Field appears in the fields list
- GraphQL field name will be: `title` (usually lowercase)

#### 3.3 Add Body Field

1. Click the **"NEW FIELD"** button again

**Field Configuration Form:**

1. **Field name:**
   - Enter: `Body`
   - This is the display name
   - Example: "Body", "Content", "Description"

2. **Data type:**
   - Select: **"Long text"** or **"Rich text (HTML)"** from the dropdown
   - **"Long text"** = plain text, multiple lines
   - **"Rich text (HTML)"** = formatted text with HTML support
   - Recommendation: Use **"Long text"** for simplicity, or **"Rich text (HTML)"** if you need formatting

3. **Required:**
   - Check the box: **"Required"** or set to **"Yes"**
   - This ensures content items have body text

4. **Field code name:**
   - May auto-generate as: `Body` or `body`
   - This is used in GraphQL queries
   - Usually lowercase version of field name
   - **Important:** Note this - you'll use it in queries (likely `body`)

**Additional Settings (if visible):**
- **Default value:** Leave empty
- **Help text:** Optional
- **Maximum length:** Optional - set if needed

5. Click **"SAVE"** to save the field

**What Happens:**
- Field is added to the content type
- Field appears in the fields list
- GraphQL field name will be: `body` (usually lowercase)

#### 3.4 Verify Fields

After adding both fields, you should see:

**Fields List:**
- **Title** (Text, Required)
- **Body** (Long text or Rich text, Required)

**Field Code Names (for GraphQL):**
- `title` - used in queries as `title`
- `body` - used in queries as `body`

**GraphQL Query Structure:**
```graphql
{
  articleArticle {
    title
    body
  }
}
```

**Important Notes:**
- Field code names are usually lowercase
- String fields (Text, Long text) are queried directly (no `{ value }` wrapper)
- The order of fields doesn't matter for queries

**What's Next:**
- Content type now has fields defined
- Next: Assign content type to channel (Step 4)
- After assignment, you can create content items

---

## Step 4: Assign Content Type to Channel

### Purpose
This step makes your content type available in the headless channel. Without this assignment, the content type cannot be used in that channel, and the GraphQL endpoint remains inactive.

### Why This Step is Needed
- Channels need to know which content types they can use
- The GraphQL endpoint is inactive until at least one content type is assigned
- This creates the connection between channel and content structure

### Detailed Steps

#### 4.1 Navigate to Channel Allowed Content Types

1. In the left sidebar, go to: **Configuration → Channel management**
2. Click on your channel name (e.g., **"Angular App"**)
3. You'll see the channel details page with multiple tabs
4. Click on the **"Allowed content types"** tab (or **"Content types"** tab)

**What You'll See:**
- A list of currently assigned content types (initially empty)
- A button to select/add content types
- Information about content type assignments

#### 4.2 Select Content Types

1. Look for a button labeled:
   - **"SELECT CONTENT TYPES"** or
   - **"Add content types"** or
   - **"Assign content types"**
2. Click this button

**What You'll See:**
- A dialog or page showing available content types
- A list of content types that can be assigned
- Checkboxes or selection interface
- Your "Article" content type should be visible in the list

#### 4.3 Assign Article Content Type

1. In the list of available content types, find **"Article"**
2. Check the checkbox next to **"Article"** or select it
3. You may see the full code name: **"Article.Article"**
4. Ensure it's selected/checked

**Note:** Only content types marked for "Headless" use will appear or be assignable.

#### 4.4 Save the Assignment

1. Click **"SAVE"** or **"OK"** or **"Assign"** (button name may vary)
2. Wait for the confirmation

**What Happens:**
- Content type is assigned to the channel
- GraphQL schema is updated
- Endpoint status changes from "Inactive" to "Active"
- The `articleArticle` field becomes available in GraphQL queries

#### 4.5 Verify Assignment

1. You should see **"Article"** in the list of assigned content types
2. Go back to the **"General"** tab of the channel
3. Check the **"GraphQL API endpoint"** section
4. The status should now show: **"Active"** or **"Endpoint: Active"**

**What This Means:**
- The GraphQL endpoint is now functional
- You can make API calls to retrieve content
- The endpoint URL is ready to use

**Important:**
- If the endpoint still shows "Inactive", refresh the page
- Ensure at least one content type is assigned
- The endpoint URL format: `http://localhost:43907/graphql/[channel-identifier]`

**What's Next:**
- Channel is configured and active
- Next: Generate API key (Step 5) for authentication
- Then: Create content items (Step 6)

---

## Step 5: Generate API Key

### Purpose
API keys authenticate requests to the GraphQL endpoint. Without a valid API key, all requests will be rejected with authentication errors.

### Why This Step is Needed
- Security: Prevents unauthorized access to your content
- Authentication: Required for all GraphQL API requests
- Access control: You can manage who can access the API

### Detailed Steps

#### 5.1 Navigate to Channel API Keys

1. In the left sidebar, go to: **Configuration → Channel management**
2. Click on your channel name (e.g., **"Angular App"**)
3. You'll see the channel details page with multiple tabs
4. Click on the **"API keys"** tab

**What You'll See:**
- A list of existing API keys (initially empty)
- A button to generate/create new API keys
- Information about API key management
- May show key creation date, expiration, status

#### 5.2 Generate New API Key

1. Look for a button labeled:
   - **"Generate new API key"** or
   - **"Create API key"** or
   - **"New API key"**
2. Click this button

**What You'll See:**
- A dialog or form for API key creation
- May ask for:
   - Key name/description (optional)
   - Expiration date (optional)
   - Permissions (if applicable)

#### 5.3 Configure API Key (if options available)

**If you see configuration options:**

1. **Key name/Description:**
   - Enter: `Angular App Key` (or your preferred name)
   - This helps you identify the key later
   - Optional but recommended

2. **Expiration:**
   - Leave empty for no expiration, or
   - Set a future date if you want the key to expire
   - For development, no expiration is fine

3. **Permissions:**
   - Leave default if available
   - Usually full access for headless channels

4. Click **"Generate"** or **"Create"**

**If no options are shown:**
- Simply click **"Generate"** or **"Create"**

#### 5.4 Copy and Save the API Key

**CRITICAL:** After generation, you'll see the API key displayed.

**What You'll See:**
- A long string of characters (the API key)
- Example format: `abc123def456ghi789jkl012mno345pqr678stu901vwx234yz`
- May be displayed in a text box or as plain text
- A "Copy" button may be available

**Actions to Take:**

1. **Copy the API Key:**
   - Select all the text of the API key
   - Copy it (Ctrl+C or right-click → Copy)
   - Or click the "Copy" button if available

2. **Save It Securely:**
   - Paste it into a secure location
   - Save it in your Angular environment file
   - Store it in a password manager
   - **Important:** You may not be able to see it again after closing the dialog

3. **Note the Key Details:**
   - Key name (if you set one)
   - Creation date
   - Status (should be "Active")

**Security Warning:**
- Never commit API keys to version control
- Don't share keys publicly
- Regenerate if compromised
- Use different keys for different environments (dev, staging, production)

#### 5.5 Verify API Key

1. The API key should appear in the list of API keys
2. Status should show: **"Active"**
3. You can see creation date and other details

**API Key Usage:**
- Include in HTTP requests as: `Authorization: Bearer [your-api-key]`
- Example header:
  ```
  Authorization: Bearer abc123def456ghi789jkl012mno345pqr678stu901vwx234yz
  ```

**What's Next:**
- API key is generated and ready
- Next: Create content items (Step 6)
- Then: Use the API key in your Angular application

---

## Step 6: Create Headless Content Items

### Purpose
Content items are the actual content instances. These are the articles, blog posts, or other content that will be delivered via the GraphQL API.

### Why This Step is Needed
- Without content items, the API will return empty results
- Content items contain the actual data (title, body, etc.)
- Only published content items are accessible via API

### Detailed Steps

#### 6.1 Navigate to Headless Items

1. In the left sidebar, go to: **Configuration → Channel management**
2. Click on your channel name (e.g., **"Angular App"**)
3. You'll see the channel details page with multiple tabs
4. Look for and click on: **"List of headless items"** tab

**What You'll See:**
- A list of existing headless items (initially empty)
- A button to create new headless items
- May show item status (Draft, Published)
- Content item management interface

#### 6.2 Create New Headless Item

1. Look for a button labeled:
   - **"NEW HEADLESS ITEM"** or
   - **"Create headless item"** or
   - **"New item"**
2. Click this button

**What You'll See:**
- A dialog or page asking you to select a content type
- A list of available content types
- Your "Article" content type should be visible

#### 6.3 Select Content Type

1. In the list of content types, find and select **"Article"**
2. You may see it as **"Article"** or **"Article.Article"**
3. Click on it or select it and click **"Next"** or **"Create"**

**What Happens:**
- A new content item form opens
- The form shows the fields you defined (Title, Body)
- You're ready to enter content

#### 6.4 Fill in Content Fields

You'll see a form with the fields you created.

**Fill in the fields:**

1. **Title Field:**
   - Locate the **"Title"** field
   - Enter: `My First Article` (or your preferred title)
   - This is a required field, so it must be filled

2. **Body Field:**
   - Locate the **"Body"** field
   - Enter: `This is the content of my first article.` (or your preferred content)
   - If it's a rich text field, you can format the text
   - This is also a required field

**Additional Fields (if any):**
- Fill in any other fields that are required
- Optional fields can be left empty

**Form Features:**
- Required fields are usually marked with an asterisk (*)
- You may see a character counter for text fields
- Rich text fields may have formatting toolbar

#### 6.5 Save the Content Item

1. Review all entered information
2. Ensure required fields are filled
3. Click the **"SAVE"** button (usually at the bottom of the form)

**What Happens:**
- Content item is saved
- Status is set to **"Draft"** (not yet published)
- You are redirected to the content item details or list
- The item appears in the list of headless items

**Important:**
- Draft items are **NOT** accessible via API
- You must publish the item (Step 7) for it to be available via API

#### 6.6 Create Additional Items (Optional)

You can create multiple content items:

1. Click **"NEW HEADLESS ITEM"** again
2. Select **"Article"** content type
3. Fill in different content:
   - Title: `My Second Article`
   - Body: `This is my second article content.`
4. Click **"SAVE"**

**Benefits of Multiple Items:**
- Test collection queries
- See how multiple items are returned
- Build a more realistic content set

**What's Next:**
- Content items are created but in Draft status
- Next: Publish content items (Step 7) to make them accessible via API

---

## Step 7: Publish Content Items

### Purpose
Publishing makes content items available via the GraphQL API. Only published items can be retrieved through API calls.

### Why This Step is Needed
- Draft items are not accessible via API (security/workflow feature)
- Publishing makes content "live" and available to clients
- This follows a content workflow: Create → Review → Publish

### Detailed Steps

#### 7.1 Navigate to Headless Items List

1. In the left sidebar, go to: **Configuration → Channel management**
2. Click on your channel name (e.g., **"Angular App"**)
3. Click on the **"List of headless items"** tab

**What You'll See:**
- List of all headless items you created
- Each item shows:
  - Title
  - Content type (Article)
  - Status (Draft, Published)
  - Last modified date
  - Actions (Edit, Publish, Delete, etc.)

#### 7.2 Select Item to Publish

1. Find the content item you want to publish (e.g., "My First Article")
2. You can see its status is **"Draft"**
3. Click on the item row or use the actions menu

**Alternative Methods:**
- Click the **"Publish"** button/icon next to the item
- Select the item and click a **"Publish"** button at the top
- Open the item and publish from the details page

#### 7.3 Publish the Item

1. If you clicked on the item, you'll see the item details page
2. Look for a **"PUBLISH"** button (usually prominent, at the top or bottom)
3. Click **"PUBLISH"**

**What You'll See:**
- A confirmation dialog (may appear)
- Or immediate publishing
- Status changes from "Draft" to "Published"

**Confirmation Dialog (if shown):**
- May ask: "Are you sure you want to publish this item?"
- Click **"Yes"** or **"Publish"** to confirm

#### 7.4 Verify Publication

1. Check the item status - it should now show **"Published"**
2. The item should be highlighted or marked differently
3. You may see a "Published" badge or icon

**What This Means:**
- The content item is now accessible via GraphQL API
- You can query it using the API endpoint
- The item will appear in API responses

#### 7.5 Publish Additional Items (if any)

1. Repeat the process for other content items
2. Select each item
3. Click **"PUBLISH"** for each one

**Best Practice:**
- Publish at least 2-3 items to test collection queries
- This helps verify that multiple items are returned correctly

#### 7.6 Verify All Items Are Published

1. Check the list of headless items
2. All items you want to access via API should show **"Published"** status
3. Draft items will not appear in API responses

**Important Notes:**
- **Only published items are accessible via API**
- Draft items are completely hidden from API
- You can unpublish items later if needed
- Unpublishing removes them from API responses

**What's Next:**
- All content items are published
- You can now test the API (see verification checklist)
- Use the API key and endpoint URL to query content

---

## Verification Checklist

Use this checklist to verify your Kentico admin configuration is complete and correct.

### Channel Configuration
- [ ] Channel created with name "Angular App" (or your name)
- [ ] Channel type is "Headless"
- [ ] Channel identifier (GUID) is noted and saved
- [ ] GraphQL endpoint URL is available
- [ ] Endpoint status shows "Active" (not "Inactive")

### Content Type Configuration
- [ ] Content type "Article" is created
- [ ] Code name is "Article.Article"
- [ ] "Use for" includes "Headless" (checked)
- [ ] Content type is saved successfully

### Fields Configuration
- [ ] "Title" field is added (Text type, Required)
- [ ] "Body" field is added (Long text or Rich text, Required)
- [ ] Both fields are saved
- [ ] Field code names are noted (likely `title` and `body`)

### Content Type Assignment
- [ ] "Article" content type is assigned to "Angular App" channel
- [ ] Assignment is saved
- [ ] GraphQL endpoint status changed to "Active" after assignment

### API Key Configuration
- [ ] API key is generated
- [ ] API key is copied and saved securely
- [ ] API key status shows "Active"
- [ ] API key is stored in Angular environment file

### Content Items Configuration
- [ ] At least one content item is created
- [ ] Content item has "Article" content type
- [ ] Title field is filled
- [ ] Body field is filled
- [ ] Content item is saved

### Content Publishing
- [ ] At least one content item is published
- [ ] Published items show "Published" status (not "Draft")
- [ ] Multiple items published (recommended for testing)

### Final Verification
- [ ] All steps completed successfully
- [ ] No error messages in admin panel
- [ ] Ready to test API endpoint
- [ ] API key and endpoint URL are available for use

---

## Common Admin Panel Issues

### Issue 1: Cannot See "Headless Channel" Option

**Symptoms:**
- "Headless channel" option is not visible when creating a channel
- Only "Website channel" or other types are available

**Possible Causes:**
- Kentico Xperience version doesn't support headless
- Headless feature is not enabled
- Incorrect license or edition

**Solutions:**
1. Verify you have Kentico Xperience version 13.0 or higher
2. Check that `kentico.xperience.webapp` package is installed
3. Ensure your license includes headless features
4. Contact Kentico support if issue persists

### Issue 2: Content Type Not Available in Channel Assignment

**Symptoms:**
- "Article" content type doesn't appear in the list when assigning to channel
- Cannot select the content type for assignment

**Possible Causes:**
- Content type is not marked for "Headless" use
- Content type was not saved properly
- Channel and content type mismatch

**Solutions:**
1. Go to Content types → Article → General tab
2. Verify "Use for" includes "Headless" (checkbox is checked)
3. Save the content type again
4. Try assigning to channel again

### Issue 3: GraphQL Endpoint Shows "Inactive"

**Symptoms:**
- Endpoint status is "Inactive" even after creating channel
- Cannot access the API endpoint

**Possible Causes:**
- No content types assigned to channel
- Content type assignment not saved
- Channel configuration incomplete

**Solutions:**
1. Verify at least one content type is assigned to the channel
2. Go to Channel → Allowed content types tab
3. Ensure "Article" is in the list
4. If not, assign it (see Step 4)
5. Refresh the page and check endpoint status again

### Issue 4: Cannot Generate API Key

**Symptoms:**
- "Generate API key" button is not visible
- Button is disabled or grayed out
- Error when trying to generate key

**Possible Causes:**
- Insufficient permissions
- Channel not properly configured
- System configuration issue

**Solutions:**
1. Verify you have administrator privileges
2. Ensure channel is saved and active
3. Check that content type is assigned
4. Try refreshing the page
5. Contact system administrator if issue persists

### Issue 5: Content Items Not Appearing in API

**Symptoms:**
- Content items are created but API returns empty results
- Items exist in admin but not accessible via API

**Possible Causes:**
- Content items are in "Draft" status (not published)
- Wrong channel identifier in API URL
- API key is incorrect or inactive

**Solutions:**
1. **Check item status:** Go to List of headless items → Verify status is "Published" (not "Draft")
2. **Publish items:** If status is "Draft", publish them (see Step 7)
3. **Verify channel identifier:** Check that you're using the correct GUID in the API URL
4. **Verify API key:** Ensure the API key is correct and active
5. **Test in Postman:** Use Postman to test the API directly before using in Angular

### Issue 6: Fields Not Appearing in GraphQL Query

**Symptoms:**
- GraphQL query returns data but fields are missing
- Error: "Field 'fieldName' does not exist"

**Possible Causes:**
- Wrong field name in query
- Field code name is different than expected
- Field not saved properly

**Solutions:**
1. **Check field code names:** Go to Content types → Article → Fields tab → Note the exact code names
2. **Use correct names:** Field code names are usually lowercase (e.g., `title`, `body`)
3. **Query structure:** For String fields, query directly: `{ articleArticle { title body } }`
4. **Verify fields:** Ensure fields are saved and content type is assigned to channel

### Issue 7: "Use for Headless" Option Not Visible

**Symptoms:**
- Cannot find "Use for" section when creating content type
- "Headless" checkbox is not available

**Possible Causes:**
- Content type form layout is different
- Option is in a different location
- Feature not available in your version

**Solutions:**
1. Look for "Usage" or "Purpose" section instead of "Use for"
2. Check all tabs in the content type form
3. Verify Kentico Xperience version supports headless
4. Check documentation for your specific version

### Issue 8: Cannot Save Content Item

**Symptoms:**
- Error when trying to save content item
- Required fields validation errors
- Save button doesn't work

**Possible Causes:**
- Required fields are not filled
- Field validation errors
- System error

**Solutions:**
1. **Fill required fields:** Ensure all fields marked with asterisk (*) are filled
2. **Check validation:** Look for error messages under fields
3. **Field types:** Ensure data matches field type (e.g., text in text field)
4. **Try again:** Save again after fixing errors
5. **Refresh:** Refresh the page if issue persists

---

## Quick Reference: Navigation Paths

### Channel Management
```
Configuration → Channel management → [Channel Name]
```

### Content Types
```
Configuration → Content types → [Content Type Name]
```

### Headless Items
```
Configuration → Channel management → [Channel Name] → List of headless items
```

### API Keys
```
Configuration → Channel management → [Channel Name] → API keys
```

### Allowed Content Types
```
Configuration → Channel management → [Channel Name] → Allowed content types
```

---

## Summary

This guide covered the complete process of configuring Kentico Xperience Headless CMS through the admin panel:

1. **Created Headless Channel** - Provides the GraphQL API endpoint
2. **Created Content Type** - Defines the content structure
3. **Added Fields** - Specifies what data to store
4. **Assigned Content Type to Channel** - Makes content type available in channel
5. **Generated API Key** - Secures API access
6. **Created Content Items** - Actual content instances
7. **Published Content Items** - Makes content accessible via API

**Next Steps:**
- Test the API endpoint using Postman or PowerShell
- Integrate with your Angular application
- Use the API key and endpoint URL in your code
- Query content using GraphQL

**Important Reminders:**
- Save the channel identifier (GUID) - needed for API URL
- Save the API key securely - needed for authentication
- Only published items are accessible via API
- Field code names are used in GraphQL queries (usually lowercase)

---

*Documentation created: February 2026*
*Kentico Xperience Version: 31.1.2*
