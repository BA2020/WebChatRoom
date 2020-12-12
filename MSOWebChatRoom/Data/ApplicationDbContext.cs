using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MSOWebChatRoom.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MSOWebChatRoom.Data
{
    public class ApplicationDbContext : IdentityDbContext<ChatUser> // since that ChatUser is a type of Indentity User
    {
        public DbSet<ChatGroup> Groups { get; set; }
        public DbSet<ChatUserGroupLink> UsersGroups { get; set; }
        public DbSet<ChatMessage> Messages { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ChatMessage>()
                .HasOne<ChatUser>(a=>a.UserSender)
                .WithMany(m=>m.MessagesS)
                .HasForeignKey(f=>f.SenderId)
                ;

            builder.Entity<ChatMessage>()
               .HasOne<ChatUser>(a => a.UserReceiver)
               .WithMany(m => m.MessagesR)
               .HasForeignKey(f => f.ReceiverId)
               ;

            builder.Entity<ChatMessage>()
                .HasOne<ChatGroup>(g => g.Group)
                .WithMany(m => m.Messages)
                .HasForeignKey(f => f.GroupeId)
                ;

            /**
             * Configurate ChatUserGroupLink Table
             * **/

            builder.Entity<ChatUserGroupLink>()
                .HasKey(ug => new { ug.userId, ug.groupeId });

            builder.Entity<ChatUserGroupLink>()
               .HasOne(ug => ug.User )
                .WithMany(u => u.Cgroups)
                .HasForeignKey(ug => ug.userId);

            builder.Entity<ChatUserGroupLink>()
               .HasOne(ug => ug.Group)
                .WithMany(u => u.Cusers)
                .HasForeignKey(ug => ug.groupeId);

        }

    }
}
