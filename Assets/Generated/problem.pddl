(define (problem problem)
	(:domain domain-logistic-normal)
	(:objects
		airplane1 - airplane
		airport1 - airport
		city1 city2 - city
		location1 - location
		package1 - package
		truck1 - truck
	)
	(:init
		(in-city airport1 city1)
		(at airplane1 airport1)
		(link city1 city2)
	)
	(:goal
		(and
			(at package1 location1)
		)
	)
)
