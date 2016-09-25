# EFSqlTranslator

A standalone linq to sql translator that can be used with EF and Dapper.

Currently supported queries:

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