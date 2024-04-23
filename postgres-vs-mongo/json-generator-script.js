JG.repeat(5, 3000, {
  id: JG.guid(),
  creationTime: moment(JG.date(new Date(2023, 0, 1), new Date(2024, 4, 15))),
  amount: JG.integer(1, 5000),
  labels: {
    userId: JG.random('1', '2', '3', '4', '5', '6'),
      category: JG.random('food', 'transport', 'fun', 'home'),
    context: JG.random('regular', 'travel', 'car')
  }
});