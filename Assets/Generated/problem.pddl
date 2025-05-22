(define (problem problem)
	(:domain )
	(:objects
		floor1 floor2 floor3 - floor
		person1 person2 - person
		elevator1 elevator2 - elevator
	)
	(:init
		(at-person person1 floor1)
		(at-person person2 floor1)
		(at-elevator elevator1 floor1)
		(at-elevator elevator2 floor1)
		(target person1 floor1)
		(target person2 floor2)
		(above floor2 floor1)
		(above floor2 floor3)
	)
	(:goal
		(and
			(reached person1)
			(reached person2)
		)
	)
)
