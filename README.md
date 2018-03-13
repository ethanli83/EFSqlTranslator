# <img src="https://github.com/ethanli83/LinqRunner/blob/master/LinqRunner.Client/src/img/Butterfly.png" align="left" height="40" width="40"/>EFSqlTranslator [![Build Status](https://travis-ci.org/ethanli83/EFSqlTranslator.svg?branch=master)](https://travis-ci.org/ethanli83/EFSqlTranslator)

A standalone linq to sql translator that can be used with EF and Dapper.

The translator is a nuget libary. To use the libary, use your nuget managment tool to install package [EFSqlTranslator.Translation](https://www.nuget.org/packages/EFSqlTranslator.Translation/) and [EFSqlTranslator.EFModels](https://www.nuget.org/packages/EFSqlTranslator.EFModels/).

Check out EFSqlTranslator.ConsoleApp project to see how to setup the translator.

## I. Basic Translation
This section demostrates how the basic linq expression is translated into sql.
### 1. Basic filtering on column values in where clause
```csharp
// Linq expression:
db.Blogs.Where(b => ((b.Url != null) && b.Name.StartsWith("Ethan")) && ((b.UserId > 1) || (b.UserId < 100)))
```
```sql
-- Transalted Sql:
select b0.*
from Blogs b0
where ((b0.Url is not null) and (b0.Name like 'Ethan%')) and ((b0.UserId > 1) or (b0.UserId < 100))
```

### 2. Filter result using list of values
```csharp
// Linq expression:
var ids = new[] { 2, 3, 5 };
db.Blogs.Where(b => b.BlogId.In(new[] { 1, 2, 4 }) && b.BlogId.In(ids))
```
```sql
-- Transalted Sql:
select b0.*
from Blogs b0
where (b0.BlogId in (1, 2, 4)) and (b0.BlogId in (2, 3, 5))
```
## II. Translating Relationsheips
In this section, we will show you how relationships are translated. The basic rules are:
  1. All relations is translated into a inner join be default.
  2. If a relation is used in a Or binary expression, Select, or Group By then join type will be changed to Left Outer Join.
  3. Parent relation will be a join to the parent entity.
  4. Child relation will be converted into a sub-select, which then got joined to.

### 1. Join to a parent relation
```csharp
// Linq expression:
db.Posts.Where(p => p.Blog.Url != null)
```
```sql
-- Transalted Sql:
select p0.*
from Posts p0
inner join Blogs b0 on p0.BlogId = b0.BlogId
where b0.Url is not null
```

### 2. Join to a child relation
```csharp
// Linq expression:
db.Blogs.Where(b => b.Posts.Any(p => p.Content != null))
```
```sql
-- Transalted Sql:
select b0.*
from Blogs b0
left outer join (
    select p0.BlogId as 'BlogId_jk0'
    from Posts p0
    where p0.Content is not null
    group by p0.BlogId
) sq0 on b0.BlogId = sq0.BlogId_jk0
where sq0.BlogId_jk0 is not null
```

### 3. Use relationships in a chain
```csharp
// Linq expression:
db.Blogs.Where(b => b.User.Comments.Any(c => c.Post.Content != null))
```
```sql
-- Transalted Sql:
select b0.*
from Blogs b0
inner join Users u0 on b0.UserId = u0.UserId
left outer join (
    select c0.UserId as 'UserId_jk0'
    from Comments c0
    inner join Posts p0 on c0.PostId = p0.PostId
    where p0.Content is not null
    group by c0.UserId
) sq0 on u0.UserId = sq0.UserId_jk0
where sq0.UserId_jk0 is not null
```
## III. Translating Select
In this section, we will show you multiple ways to select data. You can basically:
  1. Translate an anonymous object by selecting columns from different table.
  2. Do multiple Selects to get the final output.
  3. Use relations in your Select method calls.

### 1. Select out only required columns
```csharp
// Linq expression:
db.Posts.Where(p => p.User.UserName != null).Select(p => new { Content = p.Content, Title = p.Title })
```
```sql
-- Transalted Sql:
select p0.Content, p0.Title
from Posts p0
inner join Users u0 on p0.UserId = u0.UserId
where u0.UserName is not null
```

### 2. Select out required columns from related entity
```csharp
// Linq expression:
db.Posts.Where(p => p.User.UserName != null).Select(p => new { Content = p.Content, UserName = p.Blog.User.UserName })
```
```sql
-- Transalted Sql:
select p0.Content, u1.UserName
from Posts p0
inner join Users u0 on p0.UserId = u0.UserId
left outer join Blogs b0 on p0.BlogId = b0.BlogId
left outer join Users u1 on b0.UserId = u1.UserId
where u0.UserName is not null
```

### 3. Translate up selection with columns and expression
```csharp
// Linq expression:
db.Posts.Where(p => p.Content != null).Select(p => new { TitleContent = p.Title + "|" + p.Content, Num = (p.BlogId / p.User.UserId) })
```
```sql
-- Transalted Sql:
select (p0.Title + '|') + p0.Content as 'TitleContent', p0.BlogId / u0.UserId as 'Num'
from Posts p0
inner join Users u0 on p0.UserId = u0.UserId
where p0.Content is not null
```

### 4. Multiple selections with selecting whole entity
This will become really useful when combining with Group By.
```csharp
// Linq expression:
db.Posts
    .Where(p => p.Content != null)
    .Select(p => new { Blog = p.Blog, UserName = p.User.UserName })
    .Select(p => new { Url = p.Blog.Url, Name = p.Blog.Name, UserName = p.UserName })
```
```sql
-- Transalted Sql:
select b0.Url, b0.Name, u0.UserName
from Posts p0
left outer join Blogs b0 on p0.BlogId = b0.BlogId
left outer join Users u0 on p0.UserId = u0.UserId
where p0.Content is not null
```
## IV. Translating GroupBy
Grouping is always used along with aggregations. In this section, we will demostrate number of
ways that you can group your data. In the next section, you will then see how the group by works
with aggregation methods.

### 1. Basic grouping on table column
```csharp
// Linq expression:
db.Posts
    .Where(p => p.Content != null)
    .GroupBy(p => p.BlogId)
    .Select(g => new { Key = g.Key })
```
```sql
-- Transalted Sql:
select p0.BlogId as 'Key'
from Posts p0
where p0.Content is not null
group by p0.BlogId
```

### 2. Using relationships in grouping
```csharp
// Linq expression:
db.Posts
    .Where(p => p.Content != null)
    .GroupBy(p => new { Url = p.Blog.Url, UserName = p.User.UserName })
    .Select(g => new { Url = g.Key.Url, UserName = g.Key.UserName })
```
```sql
-- Transalted Sql:
select b0.Url, u0.UserName
from Posts p0
left outer join Blogs b0 on p0.BlogId = b0.BlogId
left outer join Users u0 on p0.UserId = u0.UserId
where p0.Content is not null
group by b0.Url, u0.UserName
```

### 3. Group on whole entity
This feature allows developers to write sophisticated aggregtion in a much simplier way.
```csharp
// Linq expression:
db.Posts
    .Where(p => p.Content != null)
    .GroupBy(p => new { Blog = p.Blog })
    .Select(g => new { UserId = g.Key.Blog.User.UserId })
```
```sql
-- Transalted Sql:
select u0.UserId
from Posts p0
left outer join Blogs b0 on p0.BlogId = b0.BlogId
left outer join Users u0 on b0.UserId = u0.UserId
where p0.Content is not null
group by b0.BlogId, u0.UserId
```

### 4. Mix of Select and Group method calls
```csharp
// Linq expression:
db.Posts
    .Where(p => p.Content != null)
    .Select(p => new { Blog = p.Blog, User = p.User })
    .GroupBy(x => new { Blog = x.Blog })
    .Select(x => new { Url = x.Key.Blog.Url, UserName = x.Key.Blog.User.UserName })
```
```sql
-- Transalted Sql:
select sq0.Url, u0.UserName
from (
    select b0.BlogId, b0.Url, b0.UserId as 'UserId_jk0'
    from Posts p0
    left outer join Blogs b0 on p0.BlogId = b0.BlogId
    left outer join Users u0 on p0.UserId = u0.UserId
    where p0.Content is not null
) sq0
left outer join Users u0 on sq0.UserId_jk0 = u0.UserId
group by sq0.BlogId, sq0.Url, u0.UserName
```

### 5. Group On Aggregation
```csharp
// Linq expression:
db.Blogs
    .Where(b => b.Url != null)
    .GroupBy(b => new { Cnt = b.Posts.Count(), Avg = b.Posts.Average(p => p.LikeCount) })
    .Select(x => new { Cnt = x.Key.Cnt, Avg = x.Key.Avg, CommentCount = x.Sum(b => b.CommentCount) })
```
```sql
-- Transalted Sql:
select coalesce(sq0.count0, 0) as 'Cnt', coalesce(sq0.avg0, 0) as 'Avg', sum(b0.CommentCount) as 'CommentCount'
from Blogs b0
left outer join (
    select p0.BlogId as 'BlogId_jk0', count(1) as 'count0', avg(p0.LikeCount) as 'avg0'
    from Posts p0
    group by p0.BlogId
) sq0 on b0.BlogId = sq0.BlogId_jk0
where b0.Url is not null
group by coalesce(sq0.count0, 0), coalesce(sq0.avg0, 0)
```
## V. Translating Aggregtaions
In this section, we will give you several examples to show how the aggregation is translated.
We will also demostrate few powerful aggregations that you can do with this libary.

### 1. Count on basic grouping
```csharp
// Linq expression:
db.Posts
    .Where(p => p.Content != null)
    .GroupBy(p => p.BlogId)
    .Select(g => new { cnt = g.Count() })
```
```sql
-- Transalted Sql:
select count(1) as 'cnt'
from Posts p0
where p0.Content is not null
group by p0.BlogId
```

### 2. Combine aggregations in selection
```csharp
// Linq expression:
db.Posts
    .Where(p => p.Content != null)
    .GroupBy(p => p.BlogId)
    .Select(g => new { BId = g.Key, cnt = g.Count(), Exp = (g.Sum(p => p.User.UserId) * g.Count(p => p.Content.StartsWith("Ethan"))) })
```
```sql
-- Transalted Sql:
select p0.BlogId as 'BId', count(1) as 'cnt', sum(u0.UserId) * count(case
    when p0.Content like 'Ethan%' then 1
    else null
end) as 'Exp'
from Posts p0
inner join Users u0 on p0.UserId = u0.UserId
where p0.Content is not null
group by p0.BlogId
```

### 3. Count on basic grouping with condition
```csharp
// Linq expression:
db.Posts
    .Where(p => p.Content != null)
    .GroupBy(p => p.BlogId)
    .Select(g => new { BID = g.Key, cnt = g.Count(p => (p.Blog.Url != null) || (p.User.UserId > 0)) })
```
```sql
-- Transalted Sql:
select p0.BlogId as 'BID', count(case
    when (b0.Url is not null) or (u0.UserId > 0) then 1
    else null
end) as 'cnt'
from Posts p0
left outer join Blogs b0 on p0.BlogId = b0.BlogId
left outer join Users u0 on p0.UserId = u0.UserId
where p0.Content is not null
group by p0.BlogId
```

### 4. Sum on child relationship
```csharp
// Linq expression:
db.Blogs.Where(b => b.Url != null).Select(b => new { Name = b.Name, cnt = b.Posts.Sum(p => p.PostId) })
```
```sql
-- Transalted Sql:
select b0.Name, coalesce(sq0.sum0, 0) as 'cnt'
from Blogs b0
left outer join (
    select p0.BlogId as 'BlogId_jk0', sum(p0.PostId) as 'sum0'
    from Posts p0
    group by p0.BlogId
) sq0 on b0.BlogId = sq0.BlogId_jk0
where b0.Url is not null
```

### 5. Aggregate after grouping on an entity.
```csharp
// Linq expression:
db.Posts
    .Where(p => p.Content != null)
    .GroupBy(p => new { Blog = p.Blog })
    .Select(g => new { Url = g.Key.Blog.Url, UserId = g.Key.Blog.User.UserId, Cnt = g.Key.Blog.Comments.Count(c => c.User.UserName.StartsWith("Ethan")) })
```
```sql
-- Transalted Sql:
select b0.Url, u0.UserId, count(case
    when u1.UserName like 'Ethan%' then 1
    else null
end) as 'Cnt'
from Posts p0
left outer join Blogs b0 on p0.BlogId = b0.BlogId
left outer join Users u0 on b0.UserId = u0.UserId
left outer join (
    select c0.BlogId as 'BlogId_jk0', c0.UserId as 'UserId_jk0'
    from Comments c0
    group by c0.BlogId, c0.UserId
) sq0 on b0.BlogId = sq0.BlogId_jk0
left outer join Users u1 on sq0.UserId_jk0 = u1.UserId
where p0.Content is not null
group by b0.BlogId, b0.Url, u0.UserId
```
## VI. Translating OrderBys
This section demostrates how the OrderBy is translated into sql.
### 1. OrderBy on normal column
```csharp
// Linq expression:
db.Blogs.Where(b => b.Url.StartsWith("ethan.com")).OrderBy(b => b.User.UserName)
```
```sql
-- Transalted Sql:
select b0.*
from Blogs b0
left outer join Users u0 on b0.UserId = u0.UserId
where b0.Url like 'ethan.com%'
order by u0.UserName
```

### 2. OrderBy with different direction
```csharp
// Linq expression:
db.Blogs
    .Where(b => b.Url.StartsWith("ethan.com"))
    .OrderBy(b => b.User.UserName)
    .ThenByDescending(b => b.CommentCount)
```
```sql
-- Transalted Sql:
select b0.*
from Blogs b0
left outer join Users u0 on b0.UserId = u0.UserId
where b0.Url like 'ethan.com%'
order by u0.UserName, b0.CommentCount desc
```
## VII. Translating Includes
Not like Entity Framework, Include and ThenInclude are translated as seperated select statements.
This will give us better performance when includes a one to many relation. As we do not need to
return all parent rows repeatly, this will significantly reduce the amount of data that needs to be
returned from database.

### 1. Include an entity by parent relation
```csharp
// Linq expression:
db.Posts.Where(p => p.Blog.Url != null).Include(p => p.User)
```
```sql
-- Transalted Sql:
create temporary table if not exists Temp_Table_Posts0 as 
    select p0.PostId, p0.UserId
    from Posts p0
    inner join Blogs b0 on p0.BlogId = b0.BlogId
    where b0.Url is not null;

select p0.*
from Posts p0
inner join (
    select t0.PostId, t0._rowid_
    from Temp_Table_Posts0 t0
    group by t0.PostId, t0._rowid_
) s0 on p0.PostId = s0.PostId
order by s0._rowid_;

select u0.*
from Users u0
inner join (
    select t0.UserId
    from Temp_Table_Posts0 t0
    group by t0.UserId
) s0 on u0.UserId = s0.UserId;

drop table if exists Temp_Table_Posts0
```

### 2. Include a parent relation, then include a child relation
```csharp
// Linq expression:
db.Posts
    .Where(p => p.Blog.Url != null)
    .Include(p => p.User)
    .ThenInclude(u => u.Blogs)
```
```sql
-- Transalted Sql:
create temporary table if not exists Temp_Table_Posts0 as 
    select p0.PostId, p0.UserId
    from Posts p0
    inner join Blogs b0 on p0.BlogId = b0.BlogId
    where b0.Url is not null;

select p0.*
from Posts p0
inner join (
    select t0.PostId, t0._rowid_
    from Temp_Table_Posts0 t0
    group by t0.PostId, t0._rowid_
) s0 on p0.PostId = s0.PostId
order by s0._rowid_;

create temporary table if not exists Temp_Table_Users0 as 
    select u0.UserId
    from Users u0
    inner join (
        select t0.UserId
        from Temp_Table_Posts0 t0
        group by t0.UserId
    ) s0 on u0.UserId = s0.UserId;

select u0.*
from Users u0
inner join (
    select t0.UserId, t0._rowid_
    from Temp_Table_Users0 t0
    group by t0.UserId, t0._rowid_
) s0 on u0.UserId = s0.UserId
order by s0._rowid_;

select b0.*
from Blogs b0
inner join (
    select t0.UserId
    from Temp_Table_Users0 t0
    group by t0.UserId
) s0 on b0.UserId = s0.UserId;

drop table if exists Temp_Table_Users0;

drop table if exists Temp_Table_Posts0
```
## VIII. Translating Manual Join
This libary supports more complicated join. You can define your own join condition rather than
have to be limited to column pairs.

### 1. Join on custom condition
```csharp
// Linq expression:
db.Posts.Join(
    db.Blogs.Where(b => b.Posts.Any(p => p.User.UserName != null)),(p, b) => (p.BlogId == b.BlogId) && (p.User.UserName == "ethan"),(p, b) => new { PId = p.PostId, Name = b.Name },
    DbJoinType.LeftOuter)
```
```sql
-- Transalted Sql:
select p0.PostId as 'PId', sq0.Name
from Posts p0
inner join Users u0 on p0.UserId = u0.UserId
left outer join (
    select b0.Name, b0.BlogId as 'BlogId_jk0'
    from Blogs b0
    left outer join (
        select p0.BlogId as 'BlogId_jk0'
        from Posts p0
        inner join Users u0 on p0.UserId = u0.UserId
        where u0.UserName is not null
        group by p0.BlogId
    ) sq0 on b0.BlogId = sq0.BlogId_jk0
    where sq0.BlogId_jk0 is not null
) sq0 on (p0.BlogId = sq0.BlogId_jk0) and (u0.UserName = 'ethan')
```


