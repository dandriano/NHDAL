using NHDAL.Tests.Domains.EAV.Entities;
using NHDAL.Tests.Domains.Relative.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHDAL.Tests.Domains
{
    public static class DomainsHelper
    {
        public static (List<User> users, List<Post> posts, List<Comment> comments) GenerateRelativeDomainData()
        {
            var users = new List<User>()
            {
                new User { Name = "AlrigthAlrightAlright" },
                new User { Name = "SillySandler" },
                new User { Name = "PerspectiveDiCaprio" },
            };

            var userByName = users.ToDictionary(u => u.Name);

            var posts = new List<Post>
            {
                new Post
                {
                    Author = userByName["PerspectiveDiCaprio"],
                    Text = "Hello World!",
                },
                new Post
                {
                    Author = userByName["AlrigthAlrightAlright"],
                    Text = "Is anyone here?",
                },
                new Post
                {
                    Author = userByName["SillySandler"],
                    Text = "My blog is here!",
                }
            };

            // add posts to users
            foreach (var post in posts)
            {
                post.Author.Posts.Add(post);
            }

            var comments = new List<Comment>
            {
                new Comment
                {
                    Author = userByName["SillySandler"],
                    Post = posts[0],
                    Text = "Nice post!",
                },
                new Comment
                {
                    Author = userByName["AlrigthAlrightAlright"],
                    Post = posts[0],
                    Text = "Thanks for sharing.",
                },
                new Comment
                {
                    Author = userByName["PerspectiveDiCaprio"],
                    Post = posts[1],
                    Text = "Absolutely agree!",
                },
                new Comment
                {
                    Author = userByName["PerspectiveDiCaprio"],
                    Post = posts[2],
                    Text = "Good morning to you too!",
                }
            };

            // add comments to posts and users
            foreach (var comment in comments)
            {
                comment.Post.Comments.Add(comment);
                comment.Author.Comments.Add(comment);
            }

            return (users, posts, comments);
        }
        public static (List<Entity> types, List<EAV.Entities.Attribute> attributes, List<EntityRecord> records) GenerateEAVDomainData()
        {
            // entity types definitions
            var locationType = new Entity
            {
                Name = "Location",
                Description = "DWDM location point"
            };

            var connectionType = new Entity
            {
                Name = "Connection",
                Description = "Directed connection (use another instance for bidirectional)"
            };

            var types = new List<Entity> { locationType, connectionType };

            // attribute type definitions
            var nameAttr = new EAV.Entities.Attribute { Name = "name", ValueType = "string", DisplayName = "Name", EntityType = locationType };
            var weightAttr = new EAV.Entities.Attribute { Name = "weight", ValueType = "float", DisplayName = "Weight / Length", EntityType = connectionType };
            var srcAttr = new EAV.Entities.Attribute { Name = "sourceId", ValueType = "string", DisplayName = "From Vertex ID", EntityType = connectionType };
            var tgtAttr = new EAV.Entities.Attribute { Name = "targetId", ValueType = "string", DisplayName = "To Vertex ID", EntityType = connectionType };

            var attributes = new List<EAV.Entities.Attribute> { nameAttr, weightAttr, srcAttr, tgtAttr };

            // create sample "project": a graph structure
            var projectId = Guid.NewGuid();

            // sample locations
            string[] names = ["A", "B", "C", "D", "E", "F", "G"];
            var locations = names.ToDictionary(n => n, n =>
            {
                var r = new EntityRecord { ProjectId = projectId, EntityType = locationType };
                r.AttributeMap.Add(new AttributeRecord
                {
                    AttributeId = nameAttr.Id,
                    Value = $"Location {n}",
                });

                return r;
            });

            // sample connections
            var undirected = new List<(string a, string b, float weight)>
            {
                ("A", "B", 100f),
                ("A", "C", 100f),
                ("B", "D", 100f),
                ("C", "D", 100f),
                ("D", "E", 100f),
                ("E", "C", 100f),
                ("E", "F", 100f),
                ("F", "G", 100f),
                ("G", "B", 100f),
            };

            var connections = undirected.SelectMany(u =>
            {
                (string a, string b, float w) = u;

                // edge A → B
                var edgeAB = CreateConnection(projectId, connectionType, locations[a], locations[b], w, weightAttr, srcAttr, tgtAttr);

                // edge B → A (symmetric / reverse)
                var edgeBA = CreateConnection(projectId, connectionType, locations[b], locations[a], w, weightAttr, srcAttr, tgtAttr);

                return new EntityRecord[] { edgeAB, edgeBA };
            });

            // wire inverse collections
            var records = locations.Values.Concat(connections).ToList();
            /*
            foreach (var record in records)
            {
                if (record.EntityType == locationType)
                    locationType.EntityRecords.Add(record);
                else
                    connectionType.EntityRecords.Add(record);
            }
            */
            return (types, attributes, records);
        }

        private static EntityRecord CreateConnection(Guid projectId, Entity connectionType, EntityRecord from, EntityRecord to, float weightValue,
            EAV.Entities.Attribute weight, EAV.Entities.Attribute source, EAV.Entities.Attribute target)
        {
            var edge = new EntityRecord { ProjectId = projectId, EntityType = connectionType };

            edge.AttributeMap.Add(new AttributeRecord
            {
                AttributeId = weight.Id,
                Value = weightValue.ToString("F1"),
            });
            edge.AttributeMap.Add(new AttributeRecord
            {
                AttributeId = source.Id,
                Value = from.Id.ToString(),
            });
            edge.AttributeMap.Add(new AttributeRecord
            {
                AttributeId = target.Id,
                Value = to.Id.ToString(),
            });

            return edge;
        }
    }
}