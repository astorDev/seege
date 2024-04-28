JG.repeat(5000, {
  id: JG.objectId(),
  amount: JG.integer(1, 5000),
  labels: {
    userId: JG.integer(1, 1000).toString(),
    category: JG.random('food', 'transport', 'fun', 'home')
  }
});