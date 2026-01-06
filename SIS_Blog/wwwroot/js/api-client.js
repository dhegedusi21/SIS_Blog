// Minimal API client for CRUD operations
const api = {
  jsonHeaders() { return { 'Content-Type': 'application/json' }; },

  getPosts() { return fetch('/api/posts', { credentials: 'include' }).then(r => r.json()); },

  login(payload) { return fetchWithCsrf('/api/auth/login', { method: 'POST', headers: this.jsonHeaders(), body: JSON.stringify(payload) }).then(r => r.json()); },
  register(payload) { return fetchWithCsrf('/api/auth/register', { method: 'POST', headers: this.jsonHeaders(), body: JSON.stringify(payload) }).then(r => r.json()); },

  createPost(payload) {
    return fetchWithCsrf('/api/posts', { method: 'POST', headers: this.jsonHeaders(), body: JSON.stringify(payload) }).then(r => r.json());
  },

  updatePost(id, payload) {
    return fetchWithCsrf(`/api/posts/${id}`, { method: 'PUT', headers: this.jsonHeaders(), body: JSON.stringify(payload) });
  },

  deletePost(id) {
    return fetchWithCsrf(`/api/posts/${id}`, { method: 'DELETE' });
  },

  createComment(payload) {
    return fetchWithCsrf('/api/comments', { method: 'POST', headers: this.jsonHeaders(), body: JSON.stringify(payload) }).then(r => r.json());
  },

  updateComment(id, payload) {
    return fetchWithCsrf(`/api/comments/${id}`, { method: 'PUT', headers: this.jsonHeaders(), body: JSON.stringify(payload) });
  },

  deleteComment(id) {
    return fetch(`/api/comments/${id}`, { method: 'DELETE', credentials: 'include' });
  }
};

// read antiforgery token from meta and attach to fetch where needed
function fetchWithCsrf(url, opts){
  opts = opts || {};
  opts.credentials = opts.credentials || 'include';
  opts.headers = opts.headers || {};
  var meta = document.querySelector('meta[name="csrf-token"]');
  if (meta){ opts.headers['X-CSRF-TOKEN'] = meta.content; }
  return fetch(url, opts);
}

// Bind simple handlers for forms rendered on server
document.addEventListener('submit', function(e){
  const form = e.target;
  if (form.matches('.api-create-post')){
    e.preventDefault();
    const title = form.querySelector('[name="Title"]').value;
    const content = form.querySelector('[name="Content"]').value;
    const userId = form.querySelector('[name="UserId"]') ? parseInt(form.querySelector('[name="UserId"]').value,10) : 0;
    api.createPost({ title, content, userId }).then(()=> window.location.href = '/Blog/Index');
  }

  if (form.matches('.api-create-comment')){
    e.preventDefault();
    const blogpostId = parseInt(form.querySelector('[name="blogpostId"]').value,10);
    const content = form.querySelector('[name="content"]').value;
    const userId = form.querySelector('[name="userId"]') ? parseInt(form.querySelector('[name="userId"]').value,10) : 0;
    api.createComment({ blogpostId, content, userId }).then(()=> location.reload());
  }
});

// Login/register handlers
document.addEventListener('click', function(e){
  if (e.target && e.target.id === 'login-btn'){
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    api.login({ email, password }).then(res => { if (res && res.id) window.location.href = '/'; else alert('Login failed'); }).catch(()=> alert('Login failed'));
  }
  if (e.target && e.target.id === 'register-btn'){
    const username = document.getElementById('reg-username').value;
    const email = document.getElementById('reg-email').value;
    const password = document.getElementById('reg-password').value;
    api.register({ username, email, password }).then(res => { if (res && res.id) { alert('Registered, please login'); window.location.href = '/User/Login'; } else alert('Register failed'); }).catch(()=> alert('Register failed'));
  }
});

// Delegate click for delete links
document.addEventListener('click', function(e){
  const el = e.target;
  if (el.matches('.api-delete-post')){
    e.preventDefault();
    if (!confirm('Delete post?')) return;
    const id = el.getAttribute('data-id');
    api.deletePost(id).then(()=> location.reload());
  }
  if (el.matches('.api-delete-comment')){
    e.preventDefault();
    if (!confirm('Delete comment?')) return;
    const id = el.getAttribute('data-id');
    api.deleteComment(id).then(()=> location.reload());
  }
});
