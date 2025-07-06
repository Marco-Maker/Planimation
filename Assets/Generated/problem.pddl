(define (problem p)
	(:domain domain-logistic-2-1)
	(:objects
		airplane1 - airplane
		airport1 airport2 - airport
		city1 city2 - city
		location1 location2 - location
		package1 - package
		truck1 - truck
	)
	(:init
		(in-city airport1 city1)
		(in-city airport2 city2)
		(in-city location2 city2)
		(in-city location1 city1)
		(at airplane1 airport1)
		(at package1 location1)
		(at truck1 location1)
		(link city1 city2)
		(link city2 city1)
		(is-petrol-station location1)
		(is-petrol-station location2)
	)
	(:goal
		(and
			(at package1 location2)
		)
	)
)
