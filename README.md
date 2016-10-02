# EFSqlTranslator

A standalone linq to sql translator that can be used with EF and Dapper.

# Currently supported queries:

## filter on basic column value

### linq expression
```c#
var query = db.Blogs.Where(b => b.Url != null);
```
### translated sql
```sql
select b.*                                                                                                              
from Blogs b                                                                                                         
where b.'Url' != null                                                                                                
```

## filter on basic column value from a parent relation

### linq expression
```c#
var query2 = db.Posts.Where(p => p.Blog.Url != null);
```
### translated sql
```sql
select p.*                                                                                                              
from Posts p                                                                                                         
inner join Blogs b0 on p.'BlogId' = b0.'BlogId'                                                                      
where b0.'Url' != null                                                                                               
```

## filter on basic column value from a child relation

### linq expression
```c#
var query3 = db.Blogs.Where(b => b.Posts.Any(p => p.Content != null));
```
### translated sql
```sql
select b.*                                                                                                                                         
from Blogs b                                                                                                                                       
left outer join (                                                                                                                                  
    select p.'BlogId'                                                                                                                              
    from Posts p                                                                                                                                   
    where p.'Content' != null                                                                                                                      
    group by p.'BlogId'                                                                                                                            
) x0 on b.'BlogId' = x0.'BlogId'                                                                                                                   
where x0.'BlogId' != null                                                                                           
```

## filter on basic column value from multiple deep relation                                                     

### linq expression
```c#
var query4 = db.Blogs.Where(b => b.User.Comments.Any(c => c.Post.Content != null));
```
### translated sql
```sql
select b.*                                                                                                                                         
from Blogs b                                                                                                                                       
inner join Users u0 on b.'UserId' = u0.'UserId'
left outer join (                                                                                   
    select c.'UserId'                                                                                                                              
    from Comments c                                                                                                                                
    inner join Posts p0 on c.'PostId' = p0.'PostId'                                                                                                
    where p0.'Content' != null                                                                                                                     
    group by c.'UserId'                                                                                                                            
) x0 on u0.'UserId' = x0.'UserId'                                                                                                                  
where x0.'UserId' != null             
```

## Manual join with custom condition, select columns as result

### linq expression
```
var query = db.Posts.
    Join(
        db.Blogs.Where(b => b.Url != null), 
        (p, b) => p.BlogId == b.BlogId && b.User.UserName == "ethan",
        (p, b) => new { PId = p.PostId, b.Name, BlogUser = b.User.UserName, PostUser = p.User.UserName },
        JoinType.LeftOuter);
```

### translated sql
```sql
select sq0.*
from (
    select p0.'PostId' as 'PId', sq0.'Name' as 'Name', sq0.'UserName' as 'BlogUser', u0.'UserName' as 'PostUser'
    from Posts p0
    inner join Users u0 on p0.'UserId' = u0.'UserId'
    left outer join (
        select b0.'Name', u0.'UserName', b0.'BlogId' as 'BlogId_jk0', u0.'UserName' as 'UserName_jk0'
        from Blogs b0
        inner join Users u0 on b0.'UserId' = u0.'UserId'
        where b0.'Url' != null
    ) sq0 on p0.'BlogId' = sq0.'BlogId_jk0' and sq0.'UserName_jk0' = 'ethan'
) sq0
```