(define (problem problem)
	(:domain domain-elevator-numeric)
	(:objects
		elevator1 - elevator
		person1 - person
	)
	(:init
		(= (at-elevator elevator1) 1)
		(= (at-person person1) 1)
		(= (floors ) 2)
		(= (max-load elevator1) 3)
		(= (capacity elevator1) 3)
		(= (weight person1) 1)
		(= (target person1) 2)
	)
	(:goal
		(and
			(reached person1)
		)
	)
)
