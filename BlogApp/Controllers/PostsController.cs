using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlogApp.Data.Abstract;
using BlogApp.Data.Concrete.EfCore;
using BlogApp.Entity;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Controllers
{
    public class PostsController:Controller
    {
        private IPostRepository _postRepository;
        private ICommentRepository _commentRepository;
        public PostsController(IPostRepository postRepository,ICommentRepository commentRepository)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
        }
        public async Task<IActionResult> Index(string tag)
        {

            

            var posts = _postRepository.Posts.Where(i=>i.IsActive);

            if(!string.IsNullOrEmpty(tag))
            {
                posts = posts.Where(x =>x.Tags.Any(t=>t.Url ==tag));
            }

            return View(
                new PostsViewModel {
                    Posts = await posts.ToListAsync()

                }
            );
        }
        public async Task<IActionResult> Details(string url)
        {
            return View(await _postRepository.Posts.Include(x=>x.User).Include(x=>x.Tags).Include(x =>x.Comments).ThenInclude(x=>x.User).FirstOrDefaultAsync(a => a.Url==url));
        }
        public  IActionResult AddComment(int PostId,string Text,string Ulr)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var avatar = User.FindFirstValue(ClaimTypes.UserData);

            var entity = new Comment {
                PostId = PostId,
                Text = Text,
                PublishedOn = DateTime.Now,
                UserId= int.Parse(userId ?? ""),
                User = new User{UserName = userName,Image = avatar}
            };
            _commentRepository.CreateComment(entity);

            return RedirectToRoute("post_details",new {url = Url});

        }

         [Authorize]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        public IActionResult Create(PostCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 

                _postRepository.CreatePost(
                    new Post{
                        Title = model.Title,
                        Content = model.Content,
                        Url = model.Url,
                        UserId= int.Parse(userId ?? ""),
                        PublishedOn = DateTime.Now,
                        Image = "1.jpg",
                        IsActive = false
                    }
                );

                return RedirectToAction("Index");
            }

            return View(model);
        }

         [Authorize]
        public async Task<IActionResult> List()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? ""); 
            var role = User.FindFirstValue(ClaimTypes.Role);

            var posts = _postRepository.Posts;

            if(string.IsNullOrEmpty(role))
            {
                posts = posts.Where(i => i.UserId == userId);
            }


            return View(await posts.ToListAsync());
        }

        [Authorize]
        public IActionResult Edit(int? id)
        {
            var post = _postRepository.Posts.FirstOrDefault(i=>i.PostId == id);
            if(post == null)
            {
                return NotFound();
            }

            return View(new PostCreateViewModel{
                PostId = post.PostId,
                Title = post.Title,
                Description = post.Description,
                Content = post.Content,
                Url = post.Url,
                IsActive = post.IsActive
            });
            
        }
        [Authorize]
        [HttpPost]
        public IActionResult Edit(PostCreateViewModel model)
        {
          if(ModelState.IsValid)
          {
            var entityToUpdate = new Post {
                PostId = model.PostId,
                Title = model.Title,
                Description = model.Description,
                Content = model.Content,
                Url = model.Url
            };

            if(User.FindFirstValue(ClaimTypes.Role)=="admin")
            {
                entityToUpdate.IsActive= model.IsActive;
            }



            _postRepository.EditPost(entityToUpdate);
            return RedirectToAction("List");

          }
          return View(model);
        }




    }
}