db = db.getSiblingDB('fcg_db');

// Collections para Users
db.createCollection('users');
db.users.insertOne({
  _id: 1,
  email: "admin@fcg.com",
  name: "Admin User",
  createdAt: new Date()
});

// Collections para Catalog
db.createCollection('products');
db.products.insertOne({
  _id: 1,
  name: "Sample Game",
  description: "Sample product",
  price: 29.99,
  createdAt: new Date()
});

// Collections para Eventos
db.createCollection('events');
db.events.createIndex({ timestamp: 1 });

print("✓ MongoDB initialized successfully");
