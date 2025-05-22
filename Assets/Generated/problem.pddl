(define (problem problem)
	(:domain )
	(:objects
		floor1 floor2 floor3 - floor
		person1 - person
		elevator1 - elevator
	)
	(:init
		(at-person person1 floor1)
		(at-elevator elevator1 floor1)
		(above floor2 floor1)
		(target person1 floor3)
		(above floor3 floor2)
	)
	(:goal
		(and
			(reached person1)
		)
	)
)
