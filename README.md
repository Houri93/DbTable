# DbTable
Blazor componenet for real-time EF database tables
Blazor componenet to add a generic type table which can apply paganation and sorting while accepting newly added data in real-time
Data is queried from an EF core DbContext instance

Example:

<Table DataType="Item" DataLoader="itemLoader" ItemsPerPage="5" Class="table">

    <Column Title="Id" Property="a => a.Id" Sort="true" Filter="true">

    </Column>

    <Column Title="Name" Property="a => a.Name" Sort="true" Filter="true">
        <Template Context="a">
            @a.Name.ToLower()
        </Template>
    </Column>

    <Column Title="Created" Property="a => a.Created" Sort="true" Filter="true">

    </Column>

    <Column Title="Race" Property="a => a.Race" Sort="true" Filter="true">

    </Column>

    <Column Title="On" Property="a => a.On" Sort="true" Filter="true">

    </Column>

    <Column Title="Image" Property="a=>a.Image">
        <Template Context="a">
            <img src="data:image/png;base64, @a.Image" />
        </Template>
    </Column>

</Table>
